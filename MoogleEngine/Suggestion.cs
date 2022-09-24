using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoogleEngine
{
    public class Suggestion
    {
    
        private Dictionary<string, string> words;

        public Suggestion(string [] fileContent){
            this.words = GetWords(fileContent);
        }

        //Este metodo obtiene las palabras del documento y le asocia su version normalizada
        private static Dictionary<string, string> GetWords(string [] fileContent){
            
            Dictionary<string, string> Words = new Dictionary<string, string>();
            string [] words;
            
            for(int i = 0; i < fileContent.Length; i++){
            
                words = fileContent[i].Split();

                foreach(string w in words){
                    if(!Words.ContainsKey(w)){
                        Words.Add(w, Data.NormalizeText(w));
                    }
                }
            }

            return Words;
        }

        //Este algoritmo sirve para calcular la semejanza entre una palabra y otra
        private static int Levenshtein(string st1, string st2){
            //tomamos las longitudes de las palabras
            int m = st1.Length;
            int n = st2.Length;
            int cost = 0;
            
            //luego inicializamos una matriz de m + 1 y n + 1
            int[,] matrix = new int [m + 1, n + 1];

            //si alguna de las cadenas tiene longitud cero, es porque esta vacia, luego nos costaria
            //la longitud de la otra cadena en inserciones para poder igualarlas.
            if(n == 0) return m;
            if(m == 0) return n;

            //Luego llenamos la primera fila y la primera columna(sin incluir 0;0)
            //con valores ordenados hasta la longitud de las cadenas que representan m y n.
            for(int i = 0; i <= m; matrix[i, 0] = i++)
            for(int j = 0; j <= n; matrix[0, j] = j++);

            //recorre la matriz llenando cada uno de los pesos(longitud de las subcadenas)
            //i filas, j columnas
            for(int i = 1; i <= m; i++){

                for(int j = 1; j <= n; j++){
                    //si son iguales en posiciones equidistantes
                    //el peso es cero, de lo contrario suma 1.
                    cost = st1[i - 1] == st2[j - 1] ? 0 : 1;


                    matrix [i, j] = Math.Min(Math.Min(matrix[i - 1, j] + 1, //eliminacion
                    matrix[i, j - 1] + 1),  //insercion
                    matrix[i - 1, j - 1] + cost); //sustitucion.
                    
                }
            }
            
            //devolvemos la posicion m;n, que seria la esquina inferior derecha
            //este valor seria el coste en operaciones para poder igualar las palabras.
            return matrix[m, n];

        }

        //Este metodo es para obtener una sugerencia a una palabra en funcion de cuanto se parezca a otra    
        public string GetSuggestion(string wordQuery){
                     
            string Suggestion = ""; 
                    
            //Verificamos que la palabra no se encuentra o el string no sea empty.
            if(!this.words.ContainsKey(wordQuery) && wordQuery != string.Empty){
                
                foreach(KeyValuePair<string, string> word in this.words){
                                
                    if(Levenshtein(word.Value, wordQuery) < 3){
                        Suggestion =  word.Key;
                        break;
                    }
                }
            }
            return Suggestion;
        }
    }
}