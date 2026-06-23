CREATE PROCEDURE billDelete
    @BillId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM batteryBill WHERE billId = @BillId;
END
