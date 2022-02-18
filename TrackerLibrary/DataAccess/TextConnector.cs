using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

using TrackerLibrary.DataAccess.TextHelpers;

// TODO - TournamentModel Entries prop has null for TeamCompeting.TeamName

namespace TrackerLibrary.DataAccess
{
    public class TextConnector : IDataConnection
    {
        private const string PRIZES_FILE = "PrizeModels.csv";
        private const string PEOPLE_FILE = "PersonModels.csv";
        private const string TEAMS_FILE = "TeamModels.csv";
        private const string TOURNAMENTS_FILE = "TournamentModels.csv";
        private const string MATCHUP_FILE = "MatchupModels.csv";
        private const string MATCHUP_ENTRIES_FILE = "MatchupEntryModels.csv";

        public PrizeModel CreatePrize(PrizeModel model)
        {
            // Load the text file and convert it to List<PrizeModel>
            List<PrizeModel> prizes = PRIZES_FILE.FullFilePath().LoadFile().ConvertToPrizeModels();

            // Find the max ID
            int currentId = 1;
            if(prizes.Count > 0)
            {
                currentId = prizes.OrderByDescending(x => x.Id).First().Id + 1;
            }
            model.Id = currentId;

            // Add the new recond with new ID
            prizes.Add(model);

            // Convert the prizes to List<string>
            // Save this List<string> to the text file
            prizes.SaveToPrizeFile(PRIZES_FILE);

            return model;
        }

        public PersonModel CreatePerson(PersonModel model)
        {
            // Load the text file and convert it to List<PersonModel>
            List<PersonModel> people = PEOPLE_FILE.FullFilePath().LoadFile().ConvertToPersonModels();

            // Find the max ID
            int currentId = 1;
            if (people.Count > 0)
            {
                currentId = people.OrderByDescending(x => x.Id).First().Id + 1;
            }
            model.Id = currentId;

            // Add the new recond with new ID
            people.Add(model);

            // Convert people to List<string>
            // Save this List<string> to the text file
            people.SaveToPeopleFile(PEOPLE_FILE);

            return model;
        }

        public List<PersonModel> GetPerson_All()
        {
            return PEOPLE_FILE.FullFilePath().LoadFile().ConvertToPersonModels();
        }

        public TeamModel CreateTeam(TeamModel model)
        {
            // Load the text file and convert it to List<TeamModel>
            List<TeamModel> teams = TEAMS_FILE.FullFilePath().LoadFile().ConvertToTeamModels(PEOPLE_FILE);

            // Find the max ID
            int currentId = 1;
            if (teams.Count > 0)
            {
                currentId = teams.OrderByDescending(x => x.Id).First().Id + 1;
            }
            model.Id = currentId;

            // Add the new recond with new ID
            teams.Add(model);

            // Convert the times to List<string>
            // Save this List<string> to the text file
            teams.SaveToTeamsFile(TEAMS_FILE);

            return model;
        }

        public List<TeamModel> GetTeam_All()
        {
            //return TEAMS_FILE.FullFilePath().LoadFile().ConvertToPersonModels();
            return TEAMS_FILE.FullFilePath().LoadFile().ConvertToTeamModels(PEOPLE_FILE);
        }

        public TournamentModel CreateTournament(TournamentModel model)
        {
            List<TournamentModel> tournaments = TOURNAMENTS_FILE
                .FullFilePath()
                .LoadFile()
                .ConvertToTournamentModels(TEAMS_FILE, PEOPLE_FILE, PRIZES_FILE);

            int currentId = 1;
            if (tournaments.Count > 0)
            {
                currentId = tournaments.OrderByDescending(x => x.Id).First().Id + 1;
            }
            model.Id = currentId;

            model.SaveToRoundsFile(MATCHUP_FILE, MATCHUP_ENTRIES_FILE);

            tournaments.Add(model);

            tournaments.SaveToTournamentsFile(TOURNAMENTS_FILE);

            return model;
        }

        public List<TournamentModel> GetTournament_All()
        {
            return TOURNAMENTS_FILE.FullFilePath().LoadFile().ConvertToTournamentModels(TEAMS_FILE, PEOPLE_FILE, PRIZES_FILE);
        }

        public void UpdateMatchup(MatchupModel model)
        {
            model.UpdateMatchupToFile();
        }
    }
}
