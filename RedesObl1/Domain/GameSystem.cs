using System;
using System.Collections;
using System.Collections.Generic;

namespace Domain
{
    public class GameSystem
    {
        public List<Game> Games { get; set;}
        public List<User> Users { get; set;}

        public GameSystem(){
            Games = new List<Game>();
            Users = new List<User>();
        }

        public void AddGame(Game game){
            Games.Add(game);
        }

        public void AddUser(User user){
            Users.Add(user);
        }
    }
}