using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BestFifaTeam
{
    class Jugador
    {
        public Jugador(string line)
        {
            var array = line.Split(',');

            Id = int.Parse(array[0]);
            Nombre = array[1];
            Ranking = int.Parse(array[2]);
            Edad = int.Parse(array[4]);
            Posicion = array[3];
        }

        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Ranking { get; set; }
        public int Edad { get; set; }
        public string Posicion { get; set; }
    }
}
