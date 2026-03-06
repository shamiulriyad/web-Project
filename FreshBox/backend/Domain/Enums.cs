namespace Backend.Domain
{
    public enum UserRole { User = 0, Seller = 1, Admin = 2 }

    public enum SellerApprovalStatus { Pending = 0, Approved = 1, Rejected = 2 }

    public enum PaymentMethod { BKash, Nagad, SSLCommerz, Rocket }

    public enum PaymentStatus { Pending, Paid, Failed }

    public enum OrderStatus { Pending, Confirmed, Processing, Shipped, Delivered, Cancelled }

    public enum PayoutStatus { Requested = 0, Approved = 1, Paid = 2, Rejected = 3 }

    public enum NotificationChannel { InApp = 0, Email = 1, Push = 2, Sms = 3 }
}
