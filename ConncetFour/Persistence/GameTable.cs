namespace ConnectFour.Persistence
{
    public class GameTable
    {
        // Represents the game tabel given a valid game state (loaded from a file)

        public int X, Y;         // size
        public int time1;        // Sum of X's turns
        public int time2;        // Sum of O's turns
        public string currPlayer;// current player
        public string[] Table;   // board

        public GameTable(int x, int y, int t1, int t2, string curr)
        {
            X = x;
            Y = y;
            time1 = t1;
            time2 = t2;
            currPlayer = curr;
            Table = new string[X];
        }
    }
}
