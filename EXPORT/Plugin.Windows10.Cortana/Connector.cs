using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net;
using System.IO;

namespace Plugin.Windows10
{
    class Connector
    {
        SQLiteConnection dbconn = null;
        public class Shop
        {
            public int Id { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
           // public string [] ItemTypes { get; set; }
        }

        public Connector()
        {
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            dbconn = new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path, SQLite.Net.Interop.SQLiteOpenFlags.ReadWrite | SQLite.Net.Interop.SQLiteOpenFlags.Create);
        }
        public void Connect()
        {
            dbconn.DropTable<Shop>();
            dbconn.CreateTable<Shop>();
            dbconn.Insert(new Shop { Name = "Stacbucks", Type = "COFFEE_SHOP" });
            dbconn.Insert(new Shop { Name = "Costa", Type = "COFFEE_SHOP" });
        }


        public List<Shop> FindShopByType(string type)
        {
            return dbconn.Table<Shop>().Where(x => x.Type == type).ToList();
        }

        
    }
}
