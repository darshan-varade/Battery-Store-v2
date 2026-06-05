using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using BatteryShop.DataAccess.Models;
using BatteryShop.DataAccess.ViewModels;

namespace BatteryShop.DataAccess.DAL
{
    public class ItemDAL
    {
        private Database db;

        public ItemDAL()
        {
            this.db = DatabaseFactory.CreateDatabase();
        }
        /*
        public List<ItemModel> ItemGetList()
        {
            List<ItemModel> list = new List<ItemModel>();

            DbCommand cmd = db.GetStoredProcCommand("batteryGetList");
            db.AddInParameter(cmd, "@PageNumber", DbType.Int32, 1);
            db.AddInParameter(cmd, "@PageSize", DbType.Int32, int.MaxValue);

            using (IDataReader reader = db.ExecuteReader(cmd))
            {
                while (reader.Read())
                {
                    ItemModel item = new ItemModel
                    {
                        ItemId = Convert.ToInt32(reader["itemId"]),
                        ItemSerialNumber = reader["itemSerialNumber"].ToString(),
                        ItemBrand = reader["itemBrand"].ToString(),
                        ItemType = reader["itemType"].ToString(),
                        TransactionId = Convert.ToInt32(reader["transactionId"]),
                        IsActive = Convert.ToBoolean(reader["isActive"]),
                        CreatedAt = Convert.ToDateTime(reader["createdAt"]),
                        CreatedBy = reader["createdBy"].ToString(),
                        ModifiedAt = Convert.ToDateTime(reader["lastModifiedAt"]),
                        ModifiedBy = reader["lastModifiedBy"].ToString()
                    };
                    list.Add(item);
                }
            }
            return list;
        }
        */
        public List<ItemModel> ItemGetList(ItemViewModel ItemVM)
        {
            ItemVM.PageNumber = ItemVM.PageNumber <= 0 ? 1 : ItemVM.PageNumber ;
            ItemVM.PageSize = ItemVM.PageSize <= 0 ? 100000 : ItemVM.PageSize;
            ItemVM.SerialNumber = ItemVM.SerialNumber == null ? "" : ItemVM.SerialNumber;
            ItemVM.BrandId = ItemVM.BrandId == null || ItemVM.BrandId <= 0 ? null : ItemVM.BrandId;

            List <ItemModel> list = new List<ItemModel>();

            DbCommand cmd = db.GetStoredProcCommand("batteryGetList");

            db.AddInParameter(cmd, "@PageNumber", DbType.Int32, ItemVM.PageNumber);
            db.AddInParameter(cmd, "@PageSize", DbType.Int32, ItemVM.PageSize);
            db.AddInParameter(cmd, "@SerialNumber", DbType.String, ItemVM.SerialNumber);
            db.AddInParameter(cmd, "@BrandId", DbType.Int32, (object)ItemVM.BrandId ?? DBNull.Value);
            db.AddOutParameter(cmd, "@TotalRows", DbType.Int32, 0);

            using (IDataReader reader = db.ExecuteReader(cmd))
            {
                while (reader.Read())
                {
                    ItemModel item = new ItemModel
                    {
                        ItemId = Convert.ToInt32(reader["itemId"]),
                        ItemSerialNumber = reader["itemSerialNumber"].ToString(),
                        ItemBrand = reader["itemBrand"].ToString(),
                        ItemType = reader["itemType"].ToString(),
                        TransactionId = Convert.ToInt32(reader["transactionId"]),
                        IsActive = Convert.ToBoolean(reader["isActive"]),
                        CreatedAt = Convert.ToDateTime(reader["createdAt"]),
                        CreatedBy = reader["createdBy"].ToString(),
                        ModifiedAt = Convert.ToDateTime(reader["lastModifiedAt"]),
                        ModifiedBy = reader["lastModifiedBy"].ToString()
                    };
                    list.Add(item);
                }
            }
            ItemVM.TotalRows = Convert.ToInt32(db.GetParameterValue(cmd, "@TotalRows"));
            return list;
        }

        public List<BrandListViewModel> ItemFetchBrand()
        {
            List<BrandListViewModel> list = new List<BrandListViewModel>();

            DbCommand cmd = db.GetStoredProcCommand("FetchDistinctBrandValues");

            using (IDataReader reader = db.ExecuteReader(cmd))
            {
                while (reader.Read())
                {

                    BrandListViewModel item = new BrandListViewModel
                    {
                        BrandId = Convert.ToInt32(reader["BrandId"]),
                        BrandName = reader["BrandName"].ToString()
                        
                    };
                    list.Add(item);

                }
            }
            return list;
        }

        public List<TypeListViewModel> ItemFetchType()
        {

            List<TypeListViewModel> list = new List<TypeListViewModel>();

            DbCommand cmd = db.GetStoredProcCommand("FetchDistinctTypeValues");

            using (IDataReader reader = db.ExecuteReader(cmd))
            {
                while (reader.Read())
                {

                    TypeListViewModel item = new TypeListViewModel
                    {
                        TypeId = Convert.ToInt32(reader["TypeId"]),
                        TypeName = reader["TypeName"].ToString()

                    };
                    list.Add(item);

                }
            }

            return list;
        }

        public void deleteItem(int id)
        {
            DbCommand cmd = db.GetStoredProcCommand("batteryDeleteItem");
            db.AddInParameter(cmd, "@Id", DbType.Int32, id);
            db.ExecuteNonQuery(cmd);
        }

        public void addItems(ItemAddViewModel addItemList)
        {
            foreach (var item in addItemList.Items)
            {
                DbCommand cmd = db.GetStoredProcCommand("batteryAddItem");

                db.AddInParameter(cmd, "@TransactionId",DbType.Int32, addItemList.TransactionId);
                db.AddInParameter(cmd, "@SerialNumber",DbType.String, item.SerialNumber);
                db.AddInParameter(cmd, "@BrandId",DbType.Int32, item.BrandId);
                db.AddInParameter(cmd, "@TypeId",DbType.Int32, item.TypeId);
                db.AddInParameter(cmd, "@CreatedBy", DbType.Int32, 1);
                db.ExecuteNonQuery(cmd);
            }
        }

    }
}
