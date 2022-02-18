using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess.TextHelpers
{
    public static class TextConnectorProcessor
    {
        /// <summary>
        /// Get full text file path
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string FullFilePath(this string fileName)
        {
            return $"{ConfigurationManager.AppSettings["filePath"]}\\{fileName}";
        }

        public static List<string> LoadFile(this string file)
        {
            if (!File.Exists(file))
            {
                return new List<string>();
            }

            return File.ReadAllLines(file).ToList();
        }

        public static List<PrizeModel> ConvertToPrizeModels(this List<string> lines)
        {
            var output = new List<PrizeModel>();

            foreach (var line in lines)
            {
                string[] cols = line.Split(',');

                var p = new PrizeModel();

                p.Id = int.Parse(cols[0]);
                p.PlaceNumber = int.Parse(cols[1]);
                p.PlaceName = cols[2];
                p.PrizeAmount = decimal.Parse(cols[3]);
                p.PrizePercentage = double.Parse(cols[4]);

                output.Add(p);
            }

            return output;
        }

        public static List<PersonModel> ConvertToPersonModels(this List<string> lines)
        {
            var output = new List<PersonModel>();

            foreach (var line in lines)
            {
                string[] cols = line.Split(',');

                var p = new PersonModel();

                p.Id = int.Parse(cols[0]);
                p.FirstName = cols[1];
                p.LastName = cols[2];
                p.EmailAddress = cols[3];
                p.CellphoneNumber = cols[4];

                output.Add(p);
            }

            return output;
        }

        public static List<TeamModel> ConvertToTeamModels(this List<string> lines, string PeopleFileName)
        {
            var output = new List<TeamModel>();

            // list of people to compare their ids with ids from the List<string> lines
            // to add them into output
            List<PersonModel> p = PeopleFileName.FullFilePath().LoadFile().ConvertToPersonModels();

            foreach (var line in lines)
            {
                string[] cols = line.Split(',');

                var t = new TeamModel();

                t.Id = int.Parse(cols[0]);
                t.TeamName = cols[1];
                t.TeamMembers = new List<PersonModel>();
                if (cols.Length > 2)
                {
                    for (int i = 2; i < cols.Length; i++)
                    {
                        t.TeamMembers.Add(p.Where(x => x.Id == int.Parse(cols[i])).First());
                    }
                }

                output.Add(t);
            }

            return output;
        }

        public static void SaveToPrizeFile(this List<PrizeModel> models, string fileName)
        {
            var lines = new List<string>();

            foreach (PrizeModel p in models)
            {
                lines.Add($"{p.Id},{p.PlaceNumber},{p.PlaceName},{p.PrizeAmount},{p.PrizePercentage}");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }

        public static void SaveToPeopleFile(this List<PersonModel> models, string fileName)
        {
            var lines = new List<string>();

            foreach (PersonModel p in models)
            {
                lines.Add($"{p.Id},{p.FirstName},{p.LastName},{p.EmailAddress},{p.CellphoneNumber}");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }

        public static void SaveToTeamsFile(this List<TeamModel> models, string fileName)
        {
            var lines = new List<string>();

            foreach (TeamModel t in models)
            {
                lines.Add($"{t.Id},{t.TeamName},{ConvertPeopleListToString(t.TeamMembers)}");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }

        private static string ConvertPeopleListToString(List<PersonModel> people)
        {
            string output = "";

            if (people.Count == 0)
            {
                return "";
            }

            foreach (var p in people)
            {
                output += $"{p.Id},";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        public static List<TournamentModel> ConvertToTournamentModels(this List<string> lines, string TeamsFileName, string PeopleFileName, string PrizesFileName)
        {
            var output = new List<TournamentModel>();

            // lists of stored teams of prizes to compare their ids with ids from the List<string> lines
            // to add them into output
            List<TeamModel> teamsAll = TeamsFileName.FullFilePath().LoadFile().ConvertToTeamModels(PeopleFileName);
            List<PrizeModel> prizesAll = PrizesFileName.FullFilePath().LoadFile().ConvertToPrizeModels();
            List<MatchupModel> matchupsAll = GlobalConfig.MATCHUP_FILE.FullFilePath().LoadFile().ConvertToMatchupModels();

            foreach (var line in lines)
            {
                // id| name| fee| teamid, id, id| prizeid, id id| id, id, id^ id, id, id^ id, id, id 
                // 0 - id
                // 1 - tournament name
                // 2 - entry fee
                // 3 - comma separated teams ids
                // 4 - comma separated prizes ids
                // 5 - rounds table
                string[] cols = line.Split('|');

                // tournament
                var t = new TournamentModel();

                t.Id = int.Parse(cols[0]);
                t.TournamentName = cols[1];
                t.EntryFee = decimal.Parse(cols[2]);

                var teamColumn = cols[3].Split(",");
                foreach (var teamId in teamColumn)
                {
                    if(teamId.Length != 0)
                    {
                        t.EnteredTeams.Add(teamsAll.Where(x => x.Id == int.Parse(teamId)).First());
                    }
                }

                var prizeColumn = cols[4].Split(",");
                foreach (var prizeId in prizeColumn)
                {
                    if(prizeId.Length != 0)
                    {
                        t.Prizes.Add(prizesAll.Where(x => x.Id == int.Parse(prizeId)).First());
                    }
                }


                var roundColumn = cols[5].Split("^");
                foreach (var round in roundColumn)
                {
                    var matchupsText = round.Split(",");
                    var matchupsList = new List<MatchupModel>();
                    foreach (var matchupId in matchupsText)
                    {
                        matchupsList.Add(matchupsAll.Where(x => x.Id == int.Parse(matchupId)).First());
                    }
                    t.Rounds.Add(matchupsList);
                }

                output.Add(t);
            }
            return output;
        }

        public static void SaveToTournamentsFile(this List<TournamentModel> models, string fileName)
        {
            var lines = new List<string>();

            // id| name| fee| teamid, id, id| prizeid, id id| id, id, id^ id, id, id^ id, id, id 
            foreach (TournamentModel t in models)
            {
                lines.Add($"{t.Id}|{t.TournamentName}|{t.EntryFee}|{ConvertTeamListToString(t.EnteredTeams)}|{ConvertPrizeListToString(t.Prizes)}|{ConvertRoundsToString(t.Rounds)}");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }

        private static string ConvertTeamListToString(List<TeamModel> teams)
        {
            string output = "";

            if (teams.Count == 0)
            {
                return "";
            }

            foreach (var t in teams)
            {
                output += $"{t.Id},";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertPrizeListToString(List<PrizeModel> prizes)
        {
            string output = "";

            if (prizes.Count == 0)
            {
                return "";
            }

            foreach (var p in prizes)
            {
                output += $"{p.Id},";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertRoundsToString(List<List<MatchupModel>> rounds)
        {
            string output = "";

            if (rounds.Count == 0)
            {
                return "";
            }

            foreach (var list in rounds)
            {
                foreach (var matchup in list)
                {
                    output += $"{matchup.Id},";
                }
                output = output.Substring(0, output.Length - 1);
                output += "^";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        public static void SaveToRoundsFile(this TournamentModel model, string matchupFile, string matchupEntryFile)
        {
            // loop through each round
            foreach (var round in model.Rounds)
            {
                foreach (var matchup in round)
                {
                    // load all of the matchups from file
                    // get the top id and add one
                    // store the id
                    // save the matchup record
                    matchup.SaveMatchupToFile();


                }
            }
            // loop through each matchup
            // get the id for the new matchup and save the record
            // loop through each entry, get the id, and save it 
        }

        private static List<MatchupEntryModel> ConvertStringToMatchupEntryModels(string input)
        {
            string[] ids = input.Split(",");
            var output = new List<MatchupEntryModel>();
            //var entries = GlobalConfig.MATCHUP_ENTRIES_FILE.FullFilePath().LoadFile().ConvertToMatchupEntryModels();
            var entries = GlobalConfig.MATCHUP_ENTRIES_FILE.FullFilePath().LoadFile();
            var matchingEntries = new List<string>();
            foreach (var id in ids)
            {
                foreach (var entry in entries)
                {
                    string[] cols = entry.Split(",");
                    if(cols[0] == id)
                    {
                        matchingEntries.Add(entry);
                    }
                }
            }
            //output.Add(entries.Where(x => x.Id == int.Parse(id)).First());

            output = matchingEntries.ConvertToMatchupEntryModels();

            return output;
        }

        private static List<MatchupEntryModel> ConvertToMatchupEntryModels(this List<string> lines)
        {
            var output = new List<MatchupEntryModel>();

            foreach (var line in lines)
            {
                // id, team_id, score, parentMatchup_id
                string[] cols = line.Split(',');

                var me = new MatchupEntryModel();

                me.Id = int.Parse(cols[0]);
                if(cols[1].Length != 0)
                {
                    me.TeamCompeting = GetTeam(int.Parse(cols[1]));
                }
                me.Score = double.Parse(cols[2]);
                int parentId = 0;
                if(int.TryParse(cols[3], out parentId))
                {
                    me.ParentMatchup = GetMatchup(parentId);
                }
                else
                {
                    me.ParentMatchup = null;
                }
                

                output.Add(me);
            }

            return output;
        }

        private static MatchupModel GetMatchup(int id)
        {
            var matchups = GlobalConfig.MATCHUP_FILE.FullFilePath().LoadFile();
            foreach (var matchup in matchups)
            {
                string[] cols = matchup.Split("|");
                if (cols[0] == id.ToString())
                {
                    //var matchingMatchups = new List<string>({ matchup });
                    var matchingMatchups = new List<string>();
                    matchingMatchups.Add(matchup);
                    return matchingMatchups.ConvertToMatchupModels().First();
                }
            }

            return null;
        }

        private static TeamModel GetTeam(int id)
        {
            var teams = GlobalConfig.TEAMS_FILE.FullFilePath().LoadFile();

            foreach (var team in teams)
            {
                string[] cols = team.Split(",");
                if(cols[0] == id.ToString())
                {
                    var matchingTeams = new List<string>();
                    matchingTeams.Add(team);
                    return matchingTeams.ConvertToTeamModels(GlobalConfig.PEOPLE_FILE).First();
                }
            }

            return null;
        }
        public static List<MatchupModel> ConvertToMatchupModels(this List<string> lines)
        {
            var output = new List<MatchupModel>();

            foreach (var line in lines)
            {
                // id| entry_id, id id| winner_id| matchupRound_id
                string[] cols = line.Split('|');

                var m = new MatchupModel();

                m.Id = int.Parse(cols[0]);
                m.Entries = ConvertStringToMatchupEntryModels(cols[1]);
                //string[] entries = cols[1].Split(",");
                //foreach(var entry in entries)
                //{
                //    //m.Entries
                //}
                if (cols[2].Length != 0)
                {
                    m.Winner = GetTeam(int.Parse(cols[2]));
                }
                m.MatchupRound = int.Parse(cols[3]);

                output.Add(m);
            }

            return output;
        }

        private static void SaveMatchupToFile(this MatchupModel matchup)
        {
            var matchups = GlobalConfig.MATCHUP_FILE.FullFilePath().LoadFile().ConvertToMatchupModels();

            int currentId = 1;
            if (matchups.Count > 0)
            {
                currentId = matchups.OrderByDescending(x => x.Id).First().Id + 1;
            }
            matchup.Id = currentId;

            matchups.Add(matchup);

            foreach (var entry in matchup.Entries)
            {
                entry.SaveEntryToFile();
            }

            // save to file
            var lines = new List<string>();

            // id| entry_id, id id| winner_id| matchupRound_id
            foreach (MatchupModel m in matchups)
            {
                
                string winner = "";
                if( m.Winner != null)
                {
                    winner = m.Winner.Id.ToString();
                }
                lines.Add($"{m.Id}|{ConvertMatchupEntryListToString(m.Entries)}|{winner}|{m.MatchupRound}");
            }

            File.WriteAllLines(GlobalConfig.MATCHUP_FILE.FullFilePath(), lines);
        }

        public static void UpdateMatchupToFile(this MatchupModel matchup)
        {
            var matchups = GlobalConfig.MATCHUP_FILE.FullFilePath().LoadFile().ConvertToMatchupModels();


            foreach (var m in matchups)
            {
                if(m.Id == matchup.Id)
                {
                    matchups.Remove(m);
                    break;
                }
            }

            matchups.Add(matchup);

            foreach (var entry in matchup.Entries)
            {
                entry.UpdateEntryToFile();
            }

            // save to file
            var lines = new List<string>();

            // id| entry_id, id id| winner_id| matchupRound_id
            foreach (MatchupModel m in matchups)
            {

                string winner = "";
                if (m.Winner != null)
                {
                    winner = m.Winner.Id.ToString();
                }
                lines.Add($"{m.Id}|{ConvertMatchupEntryListToString(m.Entries)}|{winner}|{m.MatchupRound}");
            }

            File.WriteAllLines(GlobalConfig.MATCHUP_FILE.FullFilePath(), lines);
        }

        // TODO - make IId interface to convert lists of things into comma separated id string
        private static string ConvertMatchupEntryListToString(List<MatchupEntryModel> me)
        {
            string output = "";

            if (me.Count == 0)
            {
                return "";
            }

            foreach (var t in me)
            {
                output += $"{t.Id},";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static void SaveEntryToFile(this MatchupEntryModel entry)
        {
            var entries = GlobalConfig.MATCHUP_ENTRIES_FILE.FullFilePath().LoadFile().ConvertToMatchupEntryModels();

            int currentId = 1;
            if (entries.Count > 0)
            {
                currentId = entries.OrderByDescending(x => x.Id).First().Id + 1;
            }
            entry.Id = currentId;

            entries.Add(entry);

            var lines = new List<string>();

            // id, team_id, score, parentMatchup_id
            foreach (MatchupEntryModel me in entries)
            {
                string parent = "";
                if(me.ParentMatchup != null)
                {
                    parent = me.ParentMatchup.Id.ToString();
                }
                string teamCompeting = "";
                if(me.TeamCompeting != null)
                {
                    teamCompeting = me.TeamCompeting.Id.ToString();
                }
                lines.Add($"{me.Id},{teamCompeting},{me.Score},{parent}");
            }

            File.WriteAllLines(GlobalConfig.MATCHUP_ENTRIES_FILE.FullFilePath(), lines);
        }

        private static void UpdateEntryToFile(this MatchupEntryModel entry)
        {
            var entries = GlobalConfig.MATCHUP_ENTRIES_FILE.FullFilePath().LoadFile().ConvertToMatchupEntryModels();

            foreach (var e in entries)
            {
                if(e.Id == entry.Id)
                {
                    entries.Remove(e);
                    break;
                }
            }

            entries.Add(entry);

            var lines = new List<string>();

            // id, team_id, score, parentMatchup_id
            foreach (MatchupEntryModel me in entries)
            {
                string parent = "";
                if (me.ParentMatchup != null)
                {
                    parent = me.ParentMatchup.Id.ToString();
                }
                string teamCompeting = "";
                if (me.TeamCompeting != null)
                {
                    teamCompeting = me.TeamCompeting.Id.ToString();
                }
                lines.Add($"{me.Id},{teamCompeting},{me.Score},{parent}");
            }

            File.WriteAllLines(GlobalConfig.MATCHUP_ENTRIES_FILE.FullFilePath(), lines);
        }
    }
}
