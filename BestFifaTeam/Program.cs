using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GAF;
using GAF.Operators;

namespace BestFifaTeam
{
    class Program
    {
        const string FILE_PATH = @"D:\Desarrollo\IA\BestFifaTeam\fifaPlayers.csv";
        const int CANTIDAD_EQUIPOS_INICIALES = 100;
        const int CANTIDAD_JUGADORES_POR_EQUIPO = 11;
        const int CANTIDAD_CORRIDAS = 400;
        const CrossoverType TIPO_CRUZAMIENTO = CrossoverType.SinglePoint;
        const ReplacementMethod TIPO_REEMPLAZO = ReplacementMethod.GenerationalReplacement;
        const ParentSelectionMethod METODO_SELECCION = ParentSelectionMethod.FitnessProportionateSelection;
        const double PROBA_CRUZAMIENTO = 0.8;
        const double PROBA_MUTACION = 0.05;
        const int ELITE = 20;
        const bool PERMITE_DUPLICADOS = true;

        private static int MAX_ID;
        private static Random rnd;
        private static List<Jugador> jugadores;
        private static StringBuilder archivo = new StringBuilder();

        static void Main(string[] args)
        {
            rnd = new Random();
            jugadores = getJugadores();
            MAX_ID = jugadores.Count -1;

            archivo.AppendLine(string.Format("Base de datos: {0}", FILE_PATH));
            archivo.AppendLine(string.Format("Cantidad de jugadores: {0}", jugadores.Count));
            archivo.AppendLine(string.Format("Cantidad de equipos iniciales: {0}", CANTIDAD_EQUIPOS_INICIALES));
            archivo.AppendLine(string.Format("Cantidad de iteraciones: {0}", CANTIDAD_CORRIDAS));
            archivo.AppendLine(string.Format("Tipo de cruzamiento: {0}", TIPO_CRUZAMIENTO.Equals(CrossoverType.SinglePoint) ? "Simple" : "Multipunto"));
            archivo.AppendLine(string.Format("Tipo de mutacion: Binaria"));
            archivo.AppendLine(string.Format("Tipo de seleccion: {0}", METODO_SELECCION.Equals(ParentSelectionMethod.TournamentSelection) ? "Torneo" : "Ruleta"));
            archivo.AppendLine(string.Format("Probabilidad de cruzamiento: {0}", PROBA_CRUZAMIENTO.ToString()));
            archivo.AppendLine(string.Format("Probabilidad de mutacion: {0}", PROBA_MUTACION.ToString()));
            archivo.AppendLine(string.Format("Porcentaje de poblacion que no es modificada (los mejores): {0}", ELITE.ToString()));

            var poblacionInicial = new Population();
            poblacionInicial.ParentSelectionMethod = METODO_SELECCION;

            //Armo poblacion inicial
            for (var i = 0; i < CANTIDAD_EQUIPOS_INICIALES; i++) {
                
                var cromosoma = new Chromosome();
                
                for (var j = 0; j < CANTIDAD_JUGADORES_POR_EQUIPO; j++) {
                    cromosoma.Genes.Add(new Gene(getJugadorAleatorio()));
                }

                poblacionInicial.Solutions.Add(cromosoma);
            }

            //Defino operadores geneticos
            var elite = new Elite(ELITE);
            var cruzamiento = new Crossover(PROBA_CRUZAMIENTO, PERMITE_DUPLICADOS, TIPO_CRUZAMIENTO, TIPO_REEMPLAZO);
            var mutacion = new BinaryMutate(PROBA_MUTACION);

            //Genero algoritmo genetico
            var algoritmo = new GeneticAlgorithm(poblacionInicial, funcionDeAptitud);

            //Defino acciones para que muestre mejor valor de cada generacion
            algoritmo.OnGenerationComplete += new GAF.GeneticAlgorithm.GenerationCompleteHandler(MostrarMejorEquipoDeLaGeneracion);
            algoritmo.OnRunComplete += new GAF.GeneticAlgorithm.RunCompleteHandler(MostrarEquipoGanador);

            //Agrego operadores geneticos previamente definidos
            algoritmo.Operators.Add(elite);
            algoritmo.Operators.Add(cruzamiento);
            algoritmo.Operators.Add(mutacion);

            //Corro el algoritmo y defino funcion de finalizacion
            algoritmo.Run(FinalizarEjecucion);
        }

        #region Private Methods

        private static double funcionDeAptitud(Chromosome solution)
        {
            //Primero, devuelvo cero si hay duplicados
            if (solution.Genes.Select(g => g.RealValue).ToList().Count != solution.Genes.Select(g => g.RealValue).Distinct().ToList().Count) return 0;

            double valor = 0;
            var equipo = new Equipo();

            foreach (var gen in solution.Genes)
            {
                equipo.Jugadores.Add(jugadores[(int)(gen.RealValue < 0 ? gen.RealValue * (-1) : gen.RealValue)]);
            }

            //Conformacion del equipo
            var cantArqueros = equipo.GetArqueros().Count;
            var cantDefensores = equipo.GetDefensores().Count(); 
            var cantVolantes = equipo.GetVolantes().Count(); 
            var cantDelanteros = equipo.GetDelanteros().Count(); 

            if (cantArqueros == 1) valor += 100;
            else valor -= 300;

            if (cantDefensores == 3) valor += 50;
            else if (cantDefensores == 4) valor += 100;
            else if (cantDefensores == 5) valor += 20;
            else valor -= 100;

            if (cantVolantes == 3) valor += 100;
            else if (cantVolantes == 4) valor += 60;
            else if (cantVolantes == 5) valor += 30;
            else valor -= 100;

            if (cantDelanteros == 1) valor += 50;
            else if (cantDelanteros == 2) valor += 70;
            else if (cantDelanteros == 3) valor += 100;
            else valor -= 100;

            //Promedio de edad
            var promedioDeEdad = equipo.PromedioEdad;
            if (promedioDeEdad < 20) valor += 10;
            else if (promedioDeEdad >= 20 & promedioDeEdad < 23) valor += 20;
            else if (promedioDeEdad >= 23 & promedioDeEdad < 25) valor += 40;
            else if (promedioDeEdad >= 25 & promedioDeEdad < 27) valor += 60;
            else if (promedioDeEdad >= 27 & promedioDeEdad < 29) valor += 80;
            else if (promedioDeEdad >= 29 & promedioDeEdad < 31) valor += 60;
            else if (promedioDeEdad >= 31 & promedioDeEdad < 33) valor += 40;
            else if (promedioDeEdad >= 33) valor += 10;

            //Ranking FIFA
            var promedioRanking = equipo.PromedioRanking;
            if (promedioRanking >= 82 && promedioRanking < 84) valor += 50;
            else if (promedioRanking >= 84 && promedioRanking < 87) valor += 100;
            else if (promedioRanking >= 87 && promedioRanking < 90) valor += 120;
            else if (promedioRanking >= 90) valor += 250;

            //Ranking defensores
            var promedioDefensores = equipo.PromedioDefensores;
            if (promedioDefensores >= 75 && promedioDefensores < 80) valor -= 200;
            else if (promedioDefensores >= 80 && promedioDefensores < 82) valor -= 100;
            else if (promedioDefensores >= 84 && promedioDefensores < 87) valor += 50;
            else if (promedioDefensores >= 87 && promedioDefensores < 90) valor += 80;
            else if (promedioDefensores >= 90) valor += 100;

            //Ranking volantes
            var promedioVolantes = equipo.PromedioVolantes;
            if (promedioVolantes >= 75 && promedioVolantes < 80) valor -= 200;
            else if (promedioVolantes >= 80 && promedioVolantes < 82) valor -= 100;
            else if (promedioVolantes >= 84 && promedioVolantes < 87) valor += 50;
            else if (promedioVolantes >= 87 && promedioVolantes < 90) valor += 80;
            else if (promedioVolantes >= 90) valor += 100;

            //Ranking delanteros
            var promedioDelanteros = equipo.PromedioDelanteros;
            if (promedioDelanteros >= 75 && promedioDelanteros < 80) valor -= 200;
            else if (promedioDelanteros >= 80 && promedioDelanteros < 82) valor -= 100;
            else if (promedioDelanteros >= 84 && promedioDelanteros < 87) valor += 50;
            else if (promedioDelanteros >= 87 && promedioDelanteros < 90) valor += 100;
            else if (promedioDelanteros >= 90) valor += 150;

            return valor < 0 ? 0 : valor / (1000);
        }

        private static bool FinalizarEjecucion(Population population, int currentGeneration, long currentEvaluation)
        {
            return currentGeneration > CANTIDAD_CORRIDAS;
        }

        private static void MostrarMejorEquipoDeLaGeneracion(object sender, GaEventArgs e)
        {
            var mejorEquipo = e.Population.GetTop(1)[0];
            var mostrar = string.Format("Generacion: {0}. Aptitud: {1}. Formacion: {2}", e.Generation, mejorEquipo.Fitness, getFormacion(mejorEquipo));
            Console.WriteLine(mostrar);
            archivo.AppendLine(mostrar);
        }

        private static void MostrarEquipoGanador(object sender, GaEventArgs e)
        {
            var mejorEquipo = new Equipo();
            var mejorCromosoma = e.Population.GetTop(1)[0];
            foreach (var gen in mejorCromosoma.Genes)
            {
                mejorEquipo.Jugadores.Add(jugadores[(int)(gen.RealValue < 0 ? gen.RealValue * (-1) : gen.RealValue)]);
            }

            archivo.AppendLine("EQUIPO GANADOR");

            var mostrar = string.Format("Generacion: {0}. Aptitud: {1}. Formacion:", e.Generation, mejorCromosoma.Fitness);
            Console.WriteLine(mostrar);
            archivo.AppendLine(mostrar);

            mostrar = string.Format("Arquero/s: {0}", mejorEquipo.GetNombresArqueros());
            Console.WriteLine(mostrar);
            archivo.AppendLine(mostrar);

            mostrar = string.Format("Defensor/es: {0}", mejorEquipo.GetNombresDefensores());
            Console.WriteLine(mostrar);
            archivo.AppendLine(mostrar);

            mostrar = string.Format("Volante/s: {0}", mejorEquipo.GetNombresVolantes());
            Console.WriteLine(mostrar);
            archivo.AppendLine(mostrar);

            mostrar = string.Format("Delantero/s: {0}", mejorEquipo.GetNombresDelanteros());
            Console.WriteLine(mostrar);
            archivo.AppendLine(mostrar);

            File.WriteAllText("AG-BestFifaTeam.txt", archivo.ToString());
            Console.ReadKey();
        }

        private static string getFormacion(Chromosome mejorEquipo)
        {
            var sb = new StringBuilder();

            foreach (var gen in mejorEquipo.Genes)
            {
                sb.Append(jugadores[(int)(gen.RealValue < 0 ? gen.RealValue * (-1) : gen.RealValue)].Nombre + " - ");
            }

            return sb.ToString().Substring(0, sb.ToString().LastIndexOf("-"));
        }

        private static int getJugadorAleatorio()
        {
            return rnd.Next(0, MAX_ID);
        }

        private static List<Jugador> getJugadores()
        {
            var jugadores = new List<Jugador>();

            return File.ReadAllLines(FILE_PATH).Select(l => new Jugador(l)).ToList();
        }

        #endregion
    }
}
