namespace Slot.Application.Services;

public class PaymentService(IBookingRepository bookingRepository, IPaymentRepository paymentRepository, IUserRepository userRepository, INotificationService notificationService) : IPaymentService
{
    public async Task<Result<BookingPaymentRecordResponse>> RecordBookingPaymentAsync(string userIdentityId, Guid bookingId, RecordBookingPaymentRequest request, CancellationToken ct = default)
    {
        var context = await GetBookingContextAsync(userIdentityId, bookingId, ct);
        if (!context.IsSuccess)
            return Result.Failure<BookingPaymentRecordResponse>(context.Error!);

        if (request.Amount <= 0)
            return Result.Failure<BookingPaymentRecordResponse>(Error.Validation("Amount must be greater than zero."));

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            Amount = request.Amount,
            Method = request.Method,
            Status = PaymentStatus.Paid,
            TransactionReference = string.IsNullOrWhiteSpace(request.TransactionReference) ? null : request.TransactionReference.Trim(),
            PaidAt = DateTime.UtcNow
        };

        await paymentRepository.CreateAsync(payment, ct);

        var booking2 = context.Value!;
        var payerEmail = (await userRepository.FindByIdentityIdAsync(userIdentityId, ct)).Item2?.UserIdentity?.Email ?? "A user";

        // Confirm the booking
        booking2.Status = BookingStatus.Completed;
        await bookingRepository.UpdateAsync(booking2, ct);

        // Notify the user that their booking is confirmed
        await notificationService.CreateNotificationAsync(
            booking2.UserId,
            "Booking Confirmed!",
            $"Your advance payment of PKR {request.Amount:N0} has been received. Your booking for {booking2.Ground.Name} on {booking2.BookingDate:dd MMM yyyy} is now confirmed!",
            ct);

        // Notify the ground owner about the payment
        await notificationService.CreateNotificationAsync(
            booking2.Ground.OwnerId,
            "Advance Payment Received",
            $"{payerEmail} has paid PKR {request.Amount:N0} via {request.Method} for the booking on {booking2.BookingDate:dd MMM yyyy}.",
            ct);

        return Result.Success(new BookingPaymentRecordResponse(payment.Id, payment.BookingId, payment.Amount, payment.Method, payment.Status, payment.TransactionReference, payment.PaidAt));
    }

    public async Task<Result<IReadOnlyList<BookingPaymentRecordResponse>>> GetBookingPaymentsAsync(string userIdentityId, Guid bookingId, CancellationToken ct = default)
    {
        var context = await GetBookingContextAsync(userIdentityId, bookingId, ct);
        if (!context.IsSuccess)
            return Result.Failure<IReadOnlyList<BookingPaymentRecordResponse>>(context.Error!);

        var payments = await paymentRepository.GetByBookingIdAsync(bookingId, ct);
        return Result.Success<IReadOnlyList<BookingPaymentRecordResponse>>(payments
            .Select(p => new BookingPaymentRecordResponse(p.Id, p.BookingId, p.Amount, p.Method, p.Status, p.TransactionReference, p.PaidAt))
            .ToList());
    }

    public async Task<Result<PaymentInitiationResponse>> InitiateJazzCashAsync(string userIdentityId, PaymentInitiationRequest request, CancellationToken ct = default)
        => await InitiateAsync(userIdentityId, request, PaymentMethod.JazzCash, "jazzcash", ct);

    public async Task<Result<PaymentCallbackResponse>> JazzCashCallbackAsync(JazzCashCallbackRequest request, CancellationToken ct = default)
        => await HandleCallbackAsync(request.BookingId, request.TransactionReference, request.Amount, request.Success, PaymentMethod.JazzCash, ct);

    public async Task<Result<PaymentInitiationResponse>> InitiateEasyPaisaAsync(string userIdentityId, PaymentInitiationRequest request, CancellationToken ct = default)
        => await InitiateAsync(userIdentityId, request, PaymentMethod.EasyPaisa, "easypaisa", ct);

    public async Task<Result<PaymentCallbackResponse>> EasyPaisaCallbackAsync(EasyPaisaCallbackRequest request, CancellationToken ct = default)
        => await HandleCallbackAsync(request.BookingId, request.TransactionReference, request.Amount, request.Success, PaymentMethod.EasyPaisa, ct);

    private async Task<Result<PaymentInitiationResponse>> InitiateAsync(string userIdentityId, PaymentInitiationRequest request, PaymentMethod method, string gateway, CancellationToken ct)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<PaymentInitiationResponse>(Error.NotFound("User not found."));

        var booking = await bookingRepository.FindByIdWithDetailsAsync(request.BookingId, ct);
        if (booking is null)
            return Result.Failure<PaymentInitiationResponse>(Error.NotFound("Booking not found."));

        if (booking.UserId != profile.Id)
            return Result.Failure<PaymentInitiationResponse>(Error.UnAuthorized("You are not allowed to initiate payment for this booking."));

        var amount = booking.RemainingAmount > 0 ? booking.RemainingAmount : booking.TotalAmount;
        var transactionReference = $"{gateway.ToUpperInvariant()}-{Guid.NewGuid():N}";

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Pending,
            TransactionReference = transactionReference
        };

        await paymentRepository.CreateAsync(payment, ct);

        var paymentUrl = $"https://sandbox.{gateway}.slot.local/pay?bookingId={booking.Id}&transactionReference={transactionReference}&amount={amount:0.00}";
        return Result.Success(new PaymentInitiationResponse(booking.Id, method, transactionReference, paymentUrl, amount, $"{method} payment initiated."));
    }

    private async Task<Result<PaymentCallbackResponse>> HandleCallbackAsync(Guid bookingId, string transactionReference, decimal amount, bool success, PaymentMethod method, CancellationToken ct)
    {
        if (bookingId == Guid.Empty)
            return Result.Failure<PaymentCallbackResponse>(Error.Validation("BookingId is required."));

        if (string.IsNullOrWhiteSpace(transactionReference))
            return Result.Failure<PaymentCallbackResponse>(Error.Validation("Transaction reference is required."));

        var booking = await bookingRepository.FindByIdWithDetailsAsync(bookingId, ct);
        if (booking is null)
            return Result.Failure<PaymentCallbackResponse>(Error.NotFound("Booking not found."));

        var payment = await paymentRepository.FindByBookingAndTransactionAsync(bookingId, transactionReference, ct);
        if (payment is null)
        {
            payment = new Payment
            {
                Id = Guid.NewGuid(),
                BookingId = bookingId,
                Amount = amount,
                Method = method,
                Status = success ? PaymentStatus.Paid : PaymentStatus.Failed,
                TransactionReference = transactionReference,
                PaidAt = success ? DateTime.UtcNow : null
            };

            await paymentRepository.CreateAsync(payment, ct);
        }
        else
        {
            payment.Amount = amount > 0 ? amount : payment.Amount;
            payment.Method = method;
            payment.Status = success ? PaymentStatus.Paid : PaymentStatus.Failed;
            payment.PaidAt = success ? DateTime.UtcNow : null;
            await paymentRepository.UpdateAsync(payment, ct);
        }

        if (success)
        {
            // Confirm the booking after successful payment
            booking.Status = BookingStatus.Completed;
            await bookingRepository.UpdateAsync(booking, ct);

            // Notify the user
            await notificationService.CreateNotificationAsync(
                booking.UserId,
                "Booking Confirmed!",
                $"Your payment of PKR {amount:N0} via {method} has been received. Your booking for {booking.Ground.Name} on {booking.BookingDate:dd MMM yyyy} is now confirmed!",
                ct);

            // Notify the ground owner
            await notificationService.CreateNotificationAsync(
                booking.Ground.OwnerId,
                "Advance Payment Received",
                $"A payment of PKR {amount:N0} via {method} has been received for the booking on {booking.BookingDate:dd MMM yyyy} at {booking.Ground.Name}.",
                ct);
        }

        return Result.Success(new PaymentCallbackResponse(bookingId, payment.Id, payment.Status, payment.TransactionReference!, success ? "Payment recorded successfully." : "Payment failed."));
    }

    private async Task<Result<Booking>> GetBookingContextAsync(string userIdentityId, Guid bookingId, CancellationToken ct)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<Booking>(Error.NotFound("User not found."));

        var booking = await bookingRepository.FindByIdWithDetailsAsync(bookingId, ct);
        if (booking is null)
            return Result.Failure<Booking>(Error.NotFound("Booking not found."));

        if (booking.UserId != profile.Id && booking.Ground.OwnerId != profile.Id)
            return Result.Failure<Booking>(Error.UnAuthorized("You are not allowed to access this booking."));

        return Result.Success(booking);
    }
}
