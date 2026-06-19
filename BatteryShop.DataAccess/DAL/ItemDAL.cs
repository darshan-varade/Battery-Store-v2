using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using BatteryShop.DataAccess.Models;
using BatteryShop.DataAccess.ViewModels;
using Serilog;

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
            string brandIds = ItemVM.BrandIds != null && ItemVM.BrandIds.Count > 0
                ? string.Join(",", ItemVM.BrandIds)
                : null;
            string statusIds = ItemVM.StatusIds != null && ItemVM.StatusIds.Count > 0
                ? string.Join(",", ItemVM.StatusIds)
                : null;

            List <ItemModel> list = new List<ItemModel>();

            DbCommand cmd = db.GetStoredProcCommand("batteryGetList");

            db.AddInParameter(cmd, "@PageNumber", DbType.Int32, ItemVM.PageNumber);
            db.AddInParameter(cmd, "@PageSize", DbType.Int32, ItemVM.PageSize);
            db.AddInParameter(cmd, "@SerialNumber", DbType.String, ItemVM.SerialNumber);
            db.AddInParameter(cmd, "@BrandIds", DbType.String, (object)brandIds ?? DBNull.Value);
            db.AddInParameter(cmd, "@StatusIds", DbType.String, (object)statusIds ?? DBNull.Value);
            db.AddInParameter(cmd, "@SortColumn", DbType.String, ItemVM.SortColumn ?? "itemId");
            db.AddInParameter(cmd, "@SortDirection", DbType.String, ItemVM.SortDirection ?? "ASC");
            db.AddOutParameter(cmd, "@TotalRows", DbType.Int32, 0);

            try
            {
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
                            ItemStatusId = Convert.ToInt32(reader["itemStatusId"]),
                            ItemStatusName = reader["itemStatusName"].ToString(),
                            IsActive = Convert.ToBoolean(reader["isActive"]),
                            CreatedAt = Convert.ToDateTime(reader["createdAt"]),
                            CreatedBy = reader["createdBy"].ToString(),
                            ModifiedAt = Convert.ToDateTime(reader["lastModifiedAt"]),
                            ModifiedBy = reader["lastModifiedBy"].ToString()
                        };
                        list.Add(item);
                    }
                }
                ItemVM.TotalRows = db.GetParameterValue(cmd, "@TotalRows") != DBNull.Value
                    ? Convert.ToInt32(db.GetParameterValue(cmd, "@TotalRows"))
                    : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in ItemGetList");
                throw;
            }
            return list;
        }

        public List<BrandListViewModel> ItemFetchBrand()
        {
            List<BrandListViewModel> list = new List<BrandListViewModel>();

            DbCommand cmd = db.GetStoredProcCommand("FetchDistinctBrandValues");

            try
            {
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
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in ItemFetchBrand");
                throw;
            }
            return list;
        }

        public List<TypeListViewModel> ItemFetchType()
        {

            List<TypeListViewModel> list = new List<TypeListViewModel>();

            DbCommand cmd = db.GetStoredProcCommand("FetchDistinctTypeValues");

            try
            {
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
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in ItemFetchType");
                throw;
            }

            return list;
        }

        public List<StatusListViewModel> ItemFetchStatus()
        {
            List<StatusListViewModel> list = new List<StatusListViewModel>();

            DbCommand cmd = db.GetStoredProcCommand("FetchDistinctStatusValues");

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        StatusListViewModel item = new StatusListViewModel
                        {
                            StatusId = Convert.ToInt32(reader["StatusId"]),
                            StatusName = reader["StatusName"].ToString()
                        };
                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in ItemFetchStatus");
                throw;
            }

            return list;
        }

        public void deleteItem(int id)
        {
            DbCommand cmd = db.GetStoredProcCommand("batteryDeleteItem");
            db.AddInParameter(cmd, "@Id", DbType.Int32, id);
            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in deleteItem");
                throw;
            }
        }

        public void addItems(ItemAddViewModel addItemList)
        {
            try
            {
                foreach (var item in addItemList.Items)
                {
                    DbCommand cmd = db.GetStoredProcCommand("batteryInsertOrUpdateItem");

                    db.AddInParameter(cmd, "@TransactionId",DbType.Int32, addItemList.TransactionId);
                    db.AddInParameter(cmd, "@SerialNumber",DbType.String, item.SerialNumber);
                    db.AddInParameter(cmd, "@BrandId",DbType.Int32, item.BrandId);
                    db.AddInParameter(cmd, "@TypeId",DbType.Int32, item.TypeId);
                    db.AddInParameter(cmd, "@ItemStatusId", DbType.Int32, 1);
                    db.AddInParameter(cmd, "@CreatedBy", DbType.Int32, 1);
                    db.ExecuteNonQuery(cmd);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in addItems");
                throw;
            }
        }

        public ItemUpdateViewModel GetItemForUpdate(int itemId)
        {
            ItemUpdateViewModel item = null;

            DbCommand cmd = db.GetStoredProcCommand("GetItemForUpdate");

            db.AddInParameter(cmd,"@ItemId",DbType.Int32,itemId);

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        item = new ItemUpdateViewModel
                        {
                            ItemId = Convert.ToInt32(reader["itemId"]),
                            SerialNumber = reader["itemSerialNumber"].ToString(),
                            BrandId = Convert.ToInt32(reader["itemBrand"]),
                            TypeId = Convert.ToInt32(reader["itemType"]),
                            TransactionId = Convert.ToInt32(reader["transactionId"]),
                            ItemStatusId = Convert.ToInt32(reader["itemStatusId"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetItemForUpdate");
                throw;
            }

            return item;
        }


        public void UpdateItem(ItemUpdateViewModel item)
        {
            DbCommand cmd = db.GetStoredProcCommand("batteryInsertOrUpdateItem");

            db.AddInParameter(cmd, "@ItemId", DbType.Int32, item.ItemId);
            db.AddInParameter(cmd, "@TransactionId", DbType.Int32, item.TransactionId);
            db.AddInParameter(cmd, "@SerialNumber", DbType.String, item.SerialNumber);
            db.AddInParameter(cmd, "@BrandId", DbType.Int32, item.BrandId);
            db.AddInParameter(cmd, "@TypeId", DbType.Int32, item.TypeId);
            db.AddInParameter(cmd, "@ItemStatusId", DbType.Int32, item.ItemStatusId ?? 1);
            db.AddInParameter(cmd, "@CreatedBy", DbType.Int32, 1);

            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in UpdateItem");
                throw;
            }
        }
    }
}
