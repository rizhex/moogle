using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoogleEngine
{
    public class DataTFIDF
    {
       private Dictionary<string, float> [] vectorTFIDF;

        public DataTFIDF(string [] fileContent){
            
            vectorTFIDF = GetTFIDF(GetIDF(GetPreIDF(fileContent), fileContent.Length), GetTF(GetPreTF(fileContent), fileContent));

        }

        /*Para el calculo del IDF, en primer lugar necesito la cantidad de documentos en que aparece cada palabra.
        Este metodo(GetPreIDF), le asociara a cada palabra del universo(fileContent), en cuantos documentos aparece.
        Se hara mediante un diccionario.*/
        private static Dictionary<string, int> GetPreIDF(string [] fileContent){
            
            /*Aqui declaramos preIDF, que sera el Diccionario a devolver.
            Ademas de el diccionario help, cuya funcion sera apoyar a la hora de 
            agregar las palabra a preIDF.*/
            Dictionary<string, int> preIDF = new Dictionary<string, int>();
            Dictionary<string, int> help = new Dictionary<string, int>();
            
            
            /*Comenzamos con un for para ir por cada documento.*/
            for(int i = 0; i < fileContent.Length; i++){
               
               /*Almacenamos en words las palabras del documento i.*/
                string [] words = fileContent[i].Split();

                /*Procedemos a ir palabra por palabra.*/
                foreach(var w in words){
                    
                    /*Si la palabra no aparece en preIDF, ira al else directamente, y la palabra sera agregada
                    a preIDF y help con un valor inicial de 1 y la posicion del documento en fileContent respectivamente.*/
                    if(preIDF.ContainsKey(w)){
                        
                        /*Una vez aqui verificamos que estemos en un documento diferente, sino no tiene sentido volver
                        a contar la palabra. En caso de que estemos en un documento diferente, simplemente le agregamos 1
                        al valor de la palabra en preIDF y actualizamos i en help.*/
                        if(help.GetValueOrDefault(w) < i){
                            
                            int temp = preIDF.GetValueOrDefault(w);
                            temp++;

                            preIDF.Remove(w);
                            preIDF.Add(w, temp);

                            help.Remove(w);
                            help.Add(w, i);
                        }
                        
                    }else{
                        preIDF.Add(w, 1);
                        help.Add(w, i);
                    }

                }
            }

            return preIDF;
        }

        /*Para el calculo del TF, en primer lugar necesito la cantidad de veces que aparece cada palabra en su respectivo documento.
        Este metodo(GetPreTF), le asociara a cada palabra del universo(fileContent), cuantas veces aparece en cada documento.
        Se hara mediante un array de Diccionario; cada posicion del array correspondera a un documento.*/
        private static Dictionary<string, int> [] GetPreTF(string [] fileContent){

            /*Aqui declaro preTF que sera el diccionario a devolver. 
            Ademas declaro un string de array(words) para separar las palabras de cada documento.*/
            Dictionary<string, int> [] preTF = new Dictionary<string, int>[fileContent.Length];
            string [] words;

            /*A continuacion un for para ir por cada documento.*/
            for(int i = 0; i < fileContent.Length; i++){
                
                /*Separaremos por palabras el documento.
                Inicializamos preTF en la posicion i.*/
                words = fileContent[i].Split();
                preTF[i] = new Dictionary<string, int>();

                /*Procedemos a ir palabra por palabra del documento i.*/
                foreach(string w in words){
                    /*Verificamos si la palabra esta en preTF, si no iremos directo al else y la inicializamos con un 
                    valor de 1. En caso de que vuelve a aparecer le agregamos 1 a su valor asociado.*/
                    if(preTF[i].ContainsKey(w)){
                        
                        int temp = preTF[i].GetValueOrDefault(w);
                        temp++;

                        preTF[i].Remove(w);
                        preTF[i].Add(w, temp);
                    }else{
                        preTF[i].Add(w, 1);
                    }
                }

                
            }
            return preTF;
        }

        /*Este metodo(GetIDF), apoyandose en GetPreIDF nos devuelve la frecuencia con la que aparece cada palabra
        en el universo(fileContent).*/
        private static Dictionary<string, float> GetIDF(Dictionary<string, int> preIDF, int fileNumber){
            
            /*Declaro el diccionario que devolvere(IDF)*/
            Dictionary<string, float> IDF = new Dictionary<string, float>();

            /*Procedo a ir por cada palabra(key) que existe en preIDF.*/
            foreach(var currentWord in preIDF){
                /*Le asocio a cada llave su valor de IDF, descrito por la formula que se presenta a continuacion.*/
                float idf = (float)Math.Log((float)fileNumber/(float)currentWord.Value);
                IDF.Add(currentWord.Key, idf);
                
            }

            return IDF;
        }

        /*Este metodo(GetTF), apoyandose en GetPreTF nos devuelve la frecuencia de cada termino en su respectivo documento.*/
        private static Dictionary<string, float> [] GetTF(Dictionary<string, int> [] preTF, string [] fileContent){

            /*Declaro el array de diccionarios(TF) donde cada posicion corresponde a un documento y el diccionario que 
            tiene en dicha posicion, tendra cada palabra con un valor de TF asociado.
            Declaro el array de string, sera para separar por palabras los documentos.*/
            Dictionary<string, float> [] TF = new Dictionary<string, float>[fileContent.Length];
            string [] words;

            /*A continuacion un for, donde iremos documento a documento.*/
            for(int i = 0; i < fileContent.Length; i++){

                /*Inicializo el diccionario de TF en la posicion i/
                Inicializo el array con las palabras por separado del documento que esta en fileContent en
                la posicion i(de esto solo me interesa la cantidad de palabras).*/
                TF[i] = new Dictionary<string, float>();
                words = fileContent[i].Split();

                /*Voy por cada palabra que este en preTF en la posicion i.*/
                foreach(var w in preTF[i]){
                    /*Para terminar, agregamos la palabra y le asociamos su valor de frecuencia inversa en el documento.*/
                    float tf = (float)w.Value/(float)words.Length;
                    TF[i].Add(w.Key, tf);
                    
                }
            }

            return TF;

        }
    
        /*Este metodo(GetTFIDF), apoyandose en GetIDF y GetTF nos devuelve el valor de TFIDF de cada palabra en su respectivo documento.*/
        private static Dictionary<string, float> [] GetTFIDF(Dictionary<string, float> IDF, Dictionary<string, float> [] TF){
            /*Declaro el array de diccionarios que devolvere.*/
            Dictionary<string, float>[] vectorTFIDF = new Dictionary<string, float>[TF.Length];

            /*Un bucle for para ir por cada documento.*/
            for(int i = 0; i < TF.Length; i++){
                
                /*Inicializo el diccionario en la posicion i de vectorTFIDF.*/
                vectorTFIDF[i] = new Dictionary<string, float>();
                
                /*A continuacion procedemos a agregar cada palabra con su valor de TFIDF, que consiste en multiplicar
                el valor de la frecuencia del termino por la frecuencia con la que aparece en el universo de documentos.*/
                foreach(var w in TF[i]){

                    float tfidf =  w.Value * IDF.GetValueOrDefault(w.Key);
            
                    vectorTFIDF[i].Add(w.Key,tfidf);
                }
            }

            return vectorTFIDF;
        }

        public Dictionary<string, float> [] GetTFIDF(){
            return this.vectorTFIDF;
        }
    }
}