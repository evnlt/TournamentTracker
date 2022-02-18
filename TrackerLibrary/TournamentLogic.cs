using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    public static class TournamentLogic
    {
        public static void CreateRounds(TournamentModel t)
        {
            // Order teams randomly
            t.EnteredTeams.Shuffle();
            int rounds = FindNumberOfRounds(t.EnteredTeams.Count);
            int byes = FindNumberOfByes(rounds, t.EnteredTeams.Count);

            t.Rounds.Add(CreateFirstRound(t.EnteredTeams, byes));

            CreateOtherRounds(t, rounds);

            // Check if it is big enough, if not add byes
            // Create the first round
            // Create every round after that
        }

        private static void CreateOtherRounds(TournamentModel t, int rounds)
        {
            int round = 2;
            List<MatchupModel> previousRound = t.Rounds[0];
            List<MatchupModel> currRound = new List<MatchupModel>();
            MatchupModel currMatchup = new MatchupModel();

            while (round <= rounds)
            {
                foreach (MatchupModel match in previousRound)
                {
                    currMatchup.Entries.Add(new MatchupEntryModel { ParentMatchup = match });

                    if(currMatchup.Entries.Count > 1)
                    {
                        currMatchup.MatchupRound = round;
                        currRound.Add(currMatchup);
                        currMatchup = new MatchupModel();
                    }
                }

                t.Rounds.Add(currRound);
                previousRound = currRound;

                currRound = new List<MatchupModel>();
                round++;
            }
        }

        private static List<MatchupModel> CreateFirstRound(List<TeamModel> teams, int byes)
        {
            var output = new List<MatchupModel>();
            var curr = new MatchupModel();

            foreach (var team in teams)
            {
                curr.Entries.Add(new MatchupEntryModel { TeamCompeting = team});

                if(byes > 0 || curr.Entries.Count > 1)
                {
                    curr.MatchupRound = 1;
                    output.Add(curr);
                    curr = new MatchupModel();

                    if (byes > 0)
                        byes--;
                }
            }

            return output;
        }

        private static int FindNumberOfRounds(int teamsCount)
        {
            int rounds = 0;

            while (teamsCount != 1)
            {
                if (teamsCount % 2 != 0)
                {
                    teamsCount++;
                }

                teamsCount /= 2;
                rounds++;
            }

            return rounds;
        }

        private static int FindNumberOfByes(int rounds, int numberOfTeams)
        {
            int output = 0;
            int totalTeams = 1;

            for (int i = 1; i <= rounds; i++)
            {
                totalTeams *= 2;
            }

            output = totalTeams - numberOfTeams;

            return output;
        }

        private static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

    }
}
