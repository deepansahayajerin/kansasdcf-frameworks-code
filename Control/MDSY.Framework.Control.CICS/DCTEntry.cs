using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Text;
using System.Threading;
using MDSY.Framework.Configuration.Common;

namespace MDSY.Framework.Control.CICS
{
  public class DCTEntry {
    private static Dictionary<String, DCTEntry> ENTRIES = new Dictionary<String, DCTEntry>();
    //private DbProviderFactory dbFactory { get; set; }
    //private DbConnection connection { get; set; }
    //private DbDataReader reader { get; set; }
    //private DbTransaction transaction { get; set; }
    //private DbDataAdapter dataAdapter { get; set; }
    public String DestID { set; get; }
    public String DestType { set; get; }
    public String DestFacility { set; get; }
    public String TransId { set; get; }
    public int TriggerLevel { set; get; }
    //public String UserId { set; get; }
    public String IndirectDest { set; get; }


    //static DCTEntry() {
    //}

    public static void LoadDct() {
      lock (ENTRIES) {
        LoadDctUnlocked();
      }
    }

    private static void LoadDctUnlocked() {
      string connectionString = ConfigSettings.GetConnectionStrings("DataConnectionString", "connectionString");

      if (String.IsNullOrEmpty(connectionString))
        return;

      try {
        using (SqlConnection connection = new SqlConnection(connectionString)) {
          connection.Open();
          using (SqlCommand command = new SqlCommand("select [DESTID],[DESTTYPE],[DESTFACILITY],[TRANSID],isnull([TRIGGERLEVEL], 0) [TRIGGERLEVEL],[INDIRECTDEST] from dbo.MDSY_DCT; ", connection)) {
            SqlDataReader rdr = command.ExecuteReader();
            while (rdr.Read()) {
              DCTEntry dct = new DCTEntry {
                DestID = ((string)rdr["DESTID"]).Trim().ToUpper(),
                DestType = ((string)rdr["DESTTYPE"]).Trim().ToUpper(),
                DestFacility = (rdr.IsDBNull(2)) ? "" : ((string)rdr["DESTFACILITY"]).Trim().ToUpper(),
                TransId = (rdr.IsDBNull(3)) ? "" : ((string)rdr["TRANSID"]).Trim().ToUpper(),
                TriggerLevel = (int)rdr["TRIGGERLEVEL"],
                IndirectDest = (rdr.IsDBNull(5)) ? "" : ((string)rdr["INDIRECTDEST"]).Trim().ToUpper()
                //UserId = (rs.IsDbNull(6)) ? "" : rs.getString(6).Trim().ToUpper(),
              };
              //if (dct.DestType != "INTRA" && dct.DestType != "EXTRA" && dct.DestType != "INDIRECT")
              //    Session.LogEvent("Invalid DCT entry detected " + dct.DestID + "," + dct.DestType);
              //else
              ENTRIES[dct.DestID] = dct;
            }
          }
        }
      }
      catch  {
        // log error
      }
      //finally {
      //}
    }

    public static bool DctLoaded { get { lock (ENTRIES) { return ENTRIES.Count > 0; } } }

    public static DCTEntry GetDctEntry(String _name) {
      String name = _name;
      if (_name.Length > 3) name = _name.Substring(0, 4);
      DCTEntry result;
      int recursionThreshold = 0;

      while (CheckLoaded().TryGetValue(name, out result)) {
        if (String.Compare(result.DestType, "INDIRECT", StringComparison.OrdinalIgnoreCase) == 0) {
          if (++recursionThreshold > 3) {
            //if (Session.Current.IsTracing)
            //    Session.Current.Trace("DCTEntry  Not Found Name: " + _name + ". INDIRECT recursion too deep.");
            throw new DestinationNotFound("Destination " + _name + " not found");
          }
          name = result.IndirectDest;
          continue;
        }
        else
          return result;
      }

      throw new DestinationNotFound("Destination " + _name + " not found");
    }

    public static IEnumerable<DCTEntry> Entries {
      get {
        List<DCTEntry> result = new List<DCTEntry>();
        result.AddRange(CheckLoaded().Values);
        result.Sort(new DctComparer());
        return result;
      }
    }

    private static Dictionary<String, DCTEntry> CheckLoaded() {
      if (ENTRIES.Count == 0) lock (ENTRIES) {
          if (ENTRIES.Count == 0) LoadDctUnlocked();
        }
      return ENTRIES;
    }

  }
  
  
  public class DctComparer : IComparer<DCTEntry>
    {
        public int Compare(DCTEntry _x, DCTEntry _y)
        {
            return _x.DestID.CompareTo(_y.DestID);
        }
    }

}
