CREATE PROCEDURE billGetById
    @BillId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        b.billId,
        b.userId,
        c.userFullName,
        c.userPhone,
        b.dateOfSale,
        b.totalAmount,
        b.paidAmount
    FROM batteryBill b
    INNER JOIN batteryCustomers c ON b.userId = c.userId
    WHERE b.billId = @BillId;
END
