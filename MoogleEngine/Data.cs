using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
namespace MoogleEngine
{
    public class Data
    {   
        private string [] fileAdresses;
        private string [] fileName;
        private string [] fileContent;
        private string [] normContent;
        private Dictionary<string, int []> [] wordsPosition;
        private Dictionary<string, string> [] snnipets;

        public Data(){
            
            this.fileAdresses = GetAdresses();
            this.fileContent = GetFileContent(this.fileAdresses);
            this.fileName = GetFileName(this.fileAdresses);
            this.normContent = new string[this.fileContent.Length];
            this.snnipets = GetFileSnnipets(this.fileAdresses);

            for(int i = 0; i < fileContent.Length; i++){
                
                this.normContent[i] = NormalizeText(this.fileContent[i]);
            }
           
            this.wordsPosition = WordsPosition(this.normContent);

        }

        /*En este metodo(GetAdresses) se devolvera un array de string con las direcciones de cada documento que
        se encuentre en ../Content/.*/
        private static string [] GetAdresses() {

        /*En esta declaracion, indico la ruta donde se encuentran los documentos y el formato que debe buscar.*/
        var txtFiles = Directory.GetFiles(@"../Content/", "*.txt");

        return txtFiles;
    }

        /*En este metodo(GetFileContent) se devolvera un array de string con el texto de cada documento. 
        Recibe un parametro(fileAdresses); su contenido sera lo que devuelva el metodo GetAdresses.*/
        private static string [] GetFileContent(string [] fileAdresses){
            
            /*Aqui declaramos el array donde se almacenara el texto de cada documento.*/
            string [] fileContent = new string[fileAdresses.Length];


            /*Luego procedo a almacenar el texto utilizando las direcciones, el texto se almacenara en un array de igual dimension que 
            fileAdresses y correspondera a cada posicion de una direccion en fileAdresses el texto en su respectiva posicion en fileContent.*/
            StreamReader reader;

            for(int i = 0; i < fileAdresses.Length; i++){
                
                reader = new StreamReader(fileAdresses[i]);

                fileContent[i] = reader.ReadToEnd();
            }

            return fileContent;
        }

        /*Este metodo sera util para eliminar todos los caracteres que no sean textos o numeros.*/
        public static string NormalizeText(string text){       
                   
            Regex a = new Regex("á"); 
            Regex e = new Regex("é");
            Regex i = new Regex("í");
            Regex o = new Regex("ó");
            Regex u = new Regex("ú");
            Regex n = new Regex("ñ");
            Regex simbol = new Regex(@"[^a-z 0-9]");
            
            text = text.ToLower(); 
            text = a.Replace(text, "a");
            text = e.Replace(text, "e");
            text = i.Replace(text, "i");
            text = o.Replace(text, "o");
            text = u.Replace(text, "u");
            text = n.Replace(text, "n");
            text = simbol.Replace(text, "");

             
            return text;
        }


        /*En este metodo, utilizare las direcciones para obtener los nombres de cada documento. Sera util a la 
        hora de devolver los resultados de la busqueda, pues vendrian siendo los titulos de cada resultado de busqueda.*/
        private static string [] GetFileName(string [] fileAdress){      

            string[] fileName = new string[fileAdress.Length];

            for(int i = 0; i < fileAdress.Length; i++){
        
                fileName[i] = fileAdress[i].Substring(11);
            }

            return fileName;

        }    

        /*Este metodo devuelve un array de diccionario donde cada posicion del array es un documento y se almacena en el diccionario
        las lineas sin normalizar y se les asocia su linea normalizada, esto sera util para devolver los snnipets.*/
        private static Dictionary<string, string> [] GetFileSnnipets(string [] fileAdresses){
            
            Dictionary<string, string> [] snnipets = new Dictionary<string, string>[fileAdresses.Length];
            string [] finalSnnipets = new string[fileAdresses.Length];
            string [] lines;

            for(int i = 0; i < fileAdresses.Length; i++){
                
                lines = File.ReadAllLines(fileAdresses[i]);
                snnipets[i] = new Dictionary<string, string>();

                foreach(var line in lines){
                    
                    if(!snnipets[i].ContainsKey(line)){
                     
                        snnipets[i].Add(line, NormalizeText(line));

                    }
                }
            }

            return snnipets;

        }


        /*Este metodo devuelve un array de diccionario de <string, int[]> donde cada diccionario representa un documento
        y cada string de cada diccionario es cada una de las palabras que aparecen en el documento, y les asocio en un 
        array las posiciones en que aparece en dicho documento, esto sera util para calcular la distancia entre dos palabras
        en caso de que introduzcan el operador "~" */
        private static Dictionary<string, int []> [] WordsPosition(string [] fileContent){
            //Declaramos el array de diccionario a devolver y le damos la longitud de la cantidad de documentos.
            Dictionary<string, int[]>[] wordsPosition = new Dictionary<string, int[]> [fileContent.Length]; 

            //Vamos por cada documento.
            for(int i = 0; i < fileContent.Length; i++){
                //Inicializamos el diccionario y tomamos las palabras del documento actual y las
                //dividimos y almacenamos en un array de string.
                wordsPosition[i] = new Dictionary<string, int[]>();
                string [] words = fileContent[i].Split();

                //Procedemos a ir por cada palabra que hay en words
                for(int j = 0; j < words.Length; j++){
                    
                    //Si el diccionario aun no contiene la palabra
                    //la agregamos le asociamos un array con la posicion j.
                    if(!wordsPosition[i].ContainsKey(words[j])){
                        int[] current = {j};
                        wordsPosition[i].Add(words[j], current);
                       
                    }else{

                        //Si la palabra ya esta, solo es necesario aumentar el array, pero como son
                       //inmutables habra que crear 2, uno para almacenar el viejo(old) y otro con
                       //la longitud de old + 1 (current).
                        int[] old = wordsPosition[i].GetValueOrDefault(words[j]);
                        int[] current = new int [old.Length + 1];

                        //Luego procedemos a ir por cada posicion de current;
                        for(int k = 0; k < current.Length; k++){
                            //Si aun no estamos en la ultima posicion de current(que seria la nueva a agregar)
                            //pasamos al else y ponemos la de old. En caso contrario agregamos la nueva(j).
                            if(k == current.Length - 1){
                                current[k] = j;
                            }else{
                                current[k] = old[k];
                            }
                        }

                        wordsPosition[i].Remove(words[j]);
                        wordsPosition[i].Add(words[j], current);
                        
                    }
                }
            }
            return wordsPosition;
        }



        /*Metodos para acceder a la informacion de los objetos de tipo Data*/
        public string[] GetContent(){
            return this.fileContent;
        }
        public string[] GetNormContent(){
            return this.normContent;
        }
        public string [] GetNames(){
            return this.fileName;
        }
        public string [] GetFileAdresses(){
            return this.fileAdresses;
        }
        public Dictionary<string, string>[] GetSnnipets(){
            return this.snnipets;
        }
        public Dictionary<string, int []> [] GetWordsPosition(){
            return this.wordsPosition;
        }
    }
        
}