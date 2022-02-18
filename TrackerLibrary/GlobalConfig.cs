using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TrackerLibrary.DataAccess;

namespace TrackerLibrary
{
    public static class GlobalConfig
    {
        public const string PRIZES_FILE = "PrizeModels.csv";
        public const string PEOPLE_FILE = "PersonModels.csv";
        public const string TEAMS_FILE = "TeamModels.csv";
        public const string TOURNAMENTS_FILE = "TournamentModels.csv";
        public const string MATCHUP_FILE = "MatchupModels.csv";
        public const string MATCHUP_ENTRIES_FILE = "MatchupEntryModels.csv";
        public static IDataConnection Connection { get; private set; }

        public static void InitializeConnections(DatabaseType connectionType)
        {
            if (connectionType == DatabaseType.Sql)
            {
                Connection = new SqlConnector();

            }
            else if (connectionType == DatabaseType.TextFile)
            {
                Connection = new TextConnector();
            }
        }

        public static string CnnString(string name)
        {
            
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }
    }
}
