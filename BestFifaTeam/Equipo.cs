using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BestFifaTeam
{
    class Equipo
    {
        private string[] ARQUEROS = new string[] { "GK" };
        private string[] VOLANTES = new string[] { "CAM", "CDM", "CM", "LM" };
        private string[] DEFENSORES = new string[] { "CB", "LB", "RB", "RWB" };
        private string[] DELANTEROS = new string[] { "CF", "LW", "LWB", "RM", "RW", "ST" };

        public Equipo()
        {
            Jugadores = new List<Jugador>();
        }

        public List<Jugador> Jugadores { get; set; }

        public double PromedioEdad {
            get {
                double total = Jugadores.Select(j => j.Edad).Sum();
                return total / Jugadores.Count;
            }
        }

        public double PromedioRanking
        {
            get
            {
                double total = Jugadores.Select(j => j.Ranking).Sum();
                return total / Jugadores.Count;
            }
        }

        public double PromedioDelanteros
        {
            get
            {
                var del = GetDelanteros();
                double total = del.Select(j => j.Ranking).Sum();
                return total / del.Count;
            }
        }

        public double PromedioDefensores
        {
            get
            {
                var def = GetDefensores();
                double total = def.Select(j => j.Ranking).Sum();
                return total / def.Count;
            }
        }

        public double PromedioVolantes
        {
            get
            {
                var vol = GetVolantes();
                double total = vol.Select(j => j.Ranking).Sum();
                return total / vol.Count;
            }
        }

        internal List<Jugador> GetArqueros()
        {
            return getJugadores(ARQUEROS); 
        }

        internal List<Jugador> GetDefensores()
        {
            return getJugadores(DEFENSORES); 
        }

        internal List<Jugador> GetVolantes()
        {
            return getJugadores(VOLANTES);
        }

        internal List<Jugador> GetDelanteros()
        {
            return getJugadores(DELANTEROS);
        }

        private List<Jugador> getJugadores(string[] posiciones)
        {
            return Jugadores.Where(j => posiciones.Contains(j.Posicion)).ToList();
        }

        private string getNombresJugadores(string[] posiciones)
        {
            var sb = new StringBuilder();

            foreach (var jugador in Jugadores.Where(j => posiciones.Contains(j.Posicion))){
                sb.Append(jugador.Nombre + " - ");
            }

            return sb.ToString().Substring(0, sb.ToString().LastIndexOf("-"));
        }

        internal string GetNombresArqueros()
        {
            return getNombresJugadores(ARQUEROS);
        }

        internal string GetNombresDefensores()
        {
            return getNombresJugadores(DEFENSORES);
        }

        internal string GetNombresVolantes()
        {
            return getNombresJugadores(VOLANTES);
        }

        internal string GetNombresDelanteros()
        {
            return getNombresJugadores(DELANTEROS);
        }
    }
}
