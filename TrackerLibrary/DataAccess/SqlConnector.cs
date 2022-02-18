using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;


namespace TrackerLibrary.DataAccess
{
    public class SqlConnector : IDataConnection
    {

        private const string DB_NAME = "Tournaments";
        public PrizeModel CreatePrize(PrizeModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(DB_NAME)))
            {
                var p = new DynamicParameters();
                //@PlaceNumber, @PlaceName, @PrizeAmount, @PrizePercentage
                p.Add("@PlaceNumber", model.PlaceNumber);
                p.Add("@PlaceName", model.PlaceName);
                p.Add("@PrizeAmount", model.PrizeAmount);
                p.Add("@PrizePercentage", model.PrizePercentage);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spPrizes_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");

                return model;
            }
        }

        public PersonModel CreatePerson(PersonModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(DB_NAME)))
            {
                var p = new DynamicParameters();
                p.Add("@FirstName", model.FirstName);
                p.Add("@LastName", model.LastName);
                p.Add("@EmailAddress", model.EmailAddress);
                p.Add("@CellphoneNumber", model.CellphoneNumber);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spPeople_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");

                return model;
            }
        }

        public List<PersonModel> GetPerson_All()
        {
            List<PersonModel> output;
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(DB_NAME)))
            {
                output = connection.Query<PersonModel>("dbo.spPeople_GetAll").ToList();
            }

            return output;
        }

        public TeamModel CreateTeam(TeamModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(DB_NAME)))
            {
                var t = new DynamicParameters();
                t.Add("@TeamName", model.TeamName);
                t.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTeams_Insert", t, commandType: CommandType.StoredProcedure);

                model.Id = t.Get<int>("@id");

                foreach(var member in model.TeamMembers)
                {
                    t = new DynamicParameters();
                    t.Add("@TeamId", model.Id);
                    t.Add("@PersonId", member.Id);
                    t.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                    connection.Execute("dbo.spTeamsMembers_Insert", t, commandType: CommandType.StoredProcedure);
                }

                return model;
            }
        }

        public List<TeamModel> GetTeam_All()
        {
            List<TeamModel> output;
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(DB_NAME)))
            {
                output = connection.Query<TeamModel>("dbo.spTeams_GetAll").ToList();

                foreach (TeamModel t in output)
                {
                    var p = new DynamicParameters();
                    p.Add("@TeamId", t.Id);
                    t.TeamMembers = connection.Query<PersonModel>("dbo.spTeamMembers_GetByTeam", p, commandType: CommandType.StoredProcedure).ToList();
                }
            }

            return output;
        }

        public TournamentModel CreateTournament(TournamentModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(DB_NAME)))
            {
                CreateTournamentTable(connection, model);

                CreateTournamentPrizes(connection, model);

                CreateTournamentTeams(connection, model);

                CreateTournamentRounds(connection, model);
            }
                return model;
        }

        private void CreateTournamentTable(IDbConnection connection, TournamentModel tournament)
        {
                var p = new DynamicParameters();
                //@TournamentName, @EntryFee, @Active
                p.Add("@TournamentName", tournament.TournamentName);
                p.Add("@EntryFee", tournament.EntryFee);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTournament_Insert", p, commandType: CommandType.StoredProcedure);

                tournament.Id = p.Get<int>("@id");
        }

        private void CreateTournamentPrizes(IDbConnection connection, TournamentModel tournament)
        {
                foreach (PrizeModel prize in tournament.Prizes)
                {
                    var p = new DynamicParameters();
                    p.Add("@TournamentId", tournament.Id);
                    p.Add("@PrizeId", prize.Id);

                    connection.Execute("dbo.spTournamentPrizes_Insert", p, commandType: CommandType.StoredProcedure);
                }
        }

        private void CreateTournamentTeams(IDbConnection connection, TournamentModel tournament)
        {
                foreach (TeamModel team in tournament.EnteredTeams)
                {
                    var p = new DynamicParameters();
                    p.Add("@TournamentId", tournament.Id);
                    p.Add("@TeamId", team.Id);

                    connection.Execute("dbo.spTournamentEntries_Insert", p, commandType: CommandType.StoredProcedure);
                }
        }

        private void CreateTournamentRounds(IDbConnection connection, TournamentModel tournament)
        {
            // loop through rounds
            foreach (var round in tournament.Rounds)
            {
                // loop through matchup
                foreach (var matchup in round)
                {
                    // save the matchup
                    var p = new DynamicParameters();
                    p.Add("@TournamentId", tournament.Id);
                    p.Add("@MatchupRound", matchup.MatchupRound);
                    p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                    connection.Execute("dbo.spMatchups_Insert", p, commandType: CommandType.StoredProcedure);

                    matchup.Id = p.Get<int>("@id");

                    foreach (var entry in matchup.Entries)
                    {
                        p = new DynamicParameters();
                        p.Add("@MatchupId", matchup.Id);
                        if(entry.ParentMatchup == null)
                        {
                            p.Add("@ParentMatchupId", null);
                        }
                        else
                        {
                            p.Add("@ParentMatchupId", entry.ParentMatchup.Id);
                        }

                        if(entry.TeamCompeting == null)
                        {
                            p.Add("@TeamCompetingId", null);
                        }
                        else
                        {
                            p.Add("@TeamCompetingId", entry.TeamCompeting.Id);
                        }

                        p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                        connection.Execute("dbo.spMatchupEntries_Insert", p, commandType: CommandType.StoredProcedure);

                        entry.Id = p.Get<int>("@id");
                    }
                }
            }
            // loop through matchup
            // save the matchup
            // loop through the entries and save them 
        }

        public List<TournamentModel> GetTournament_All()
        {
            List<TournamentModel> output;
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(DB_NAME)))
            {
                // get ids, names, entry fees
                output = connection.Query<TournamentModel>("dbo.spTournaments_GetAll").ToList();

                foreach (TournamentModel tour in output)
                {
                    var p1 = new DynamicParameters();
                    p1.Add("@TournamentId", tour.Id);

                    // get entered teams
                    tour.EnteredTeams = connection.Query<TeamModel>("dbo.spTournamentEntries_GetByTournament", p1, commandType: CommandType.StoredProcedure).ToList();

                    // get team members
                    foreach (TeamModel team in tour.EnteredTeams)
                    {
                        var p2 = new DynamicParameters();
                        p2.Add("@TeamId", team.Id);
                        team.TeamMembers = connection.Query<PersonModel>("dbo.spTeamMembers_GetByTeam", p2, commandType: CommandType.StoredProcedure).ToList();
                    }

                    // get prizes
                    tour.Prizes = connection.Query<PrizeModel>("dbo.spTournamentPrizes_GetByTournament", p1, commandType: CommandType.StoredProcedure).ToList();

                    // get rounds
                    var matchups = connection.Query<MatchupModel>("dbo.spMatchups_GetByTournament", p1, commandType: CommandType.StoredProcedure).ToList();

                    foreach (var m in matchups)
                    {
                        var p3 = new DynamicParameters();
                        p3.Add("@MatchupId", m.Id);
                        m.Entries = connection.Query<MatchupEntryModel>("dbo.spMatchupEntries_GetByMatchup", p3, commandType: CommandType.StoredProcedure).ToList();

                        var teamsAll = GetTeam_All();

                        if(m.WinnerId > 0)
                        {
                            m.Winner = teamsAll.Where(x => x.Id == m.WinnerId).First();
                        }

                        foreach (var me in m.Entries)
                        {
                            if(me.TeamCompetingId > 0)
                            {
                                me.TeamCompeting = teamsAll.Where(x => x.Id == me.TeamCompetingId).First();
                            }

                            if(me.ParentMatchupId > 0)
                            {
                                me.ParentMatchup = matchups.Where(x => x.Id == me.ParentMatchupId).First();
                            }
                        }
                    }


                    var currRow = new List<MatchupModel>();
                    int currRound = 1;

                    foreach (var m in matchups)
                    {
                        if(m.MatchupRound > currRound)
                        {
                            tour.Rounds.Add(currRow);
                            currRow = new List<MatchupModel>();
                            currRound++;
                        }

                        currRow.Add(m);
                    }

                    tour.Rounds.Add(currRow);

                }

            }

            return output;
        }

        public void UpdateMatchup(MatchupModel model)
        {
            // dbo.spMatchups_Update
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(DB_NAME)))
            {
                var p = new DynamicParameters();
                p.Add("@id", model.Id);
                p.Add("@WinnerId", model.WinnerId);

                connection.Execute("dbo.spMatchups_Update", p, commandType: CommandType.StoredProcedure);


                for (int i = 0; i < 2; i++)
                {
                    p = new DynamicParameters();
                    p.Add("@id", model.Entries[i].Id);
                    p.Add("@TeamCompetingId", model.Entries[i].TeamCompetingId);
                    p.Add("@Score", model.Entries[i].Score);

                    connection.Execute("dbo.spMatchupEntries_Update", p, commandType: CommandType.StoredProcedure);
                }

            }
        }
    }
}
