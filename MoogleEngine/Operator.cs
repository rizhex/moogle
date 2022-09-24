using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoogleEngine
{
    public class Operator
    {
        private char [] oper;
        private int [] countAsterisc;
        public Operator(string [] splitQuery){
            
            this.oper = CheckOperator(splitQuery);
            this.countAsterisc = GetAsterisc(splitQuery);
        }

        /*Este metodo devuelve un array de char q tendra la misma dimension de splitQuery, y nos dira si existe algun
        operador o no en sus posiciones, devovlera true en caso de que si.*/
        private static char [] CheckOperator(string [] splitQuery){
            
            char [] oper = new char [splitQuery.Length];
                
            //Procedemos a ir por cada palabra de query.
            for(int i = 0; i < splitQuery.Length; i++){
                
                //Hacemos un foreach para cada caracter, pero en realdiad solo veremos el primero y haremos un break;
                foreach(var ch in splitQuery[i]){
                        
                    if(ch == '*'){
                        
                        oper[i] = ch;
                        break;

                    }else if((ch == '^' || ch == '!')){

                        oper[i] = ch;
                        break;
                    
                    }else if(ch == '~' && splitQuery[i].Length == 1 && i > 0){
                        //Este operador es especial, pues se coloca de forma distinta, es necesario que el usuario
                        //lo coloque de forma tal q haya espacio entre el y las palabras a su alrededor, ademas 
                        //de que nunca puede ser colocado en la primera posicion, por eso la condicion(i > 0), y debe estar
                        //aislado(splitQuery[i].Length == 1).
                        oper[i] = ch;
                        break;

                    }else{
            
                        break;
                    }
                }

            }

            return oper;
        } 

        //Este metodo sera utilizado para obtener la cantidad de asteriscos que hay presentes en caso de que se devuelva true
        //y el caracter sea el asterisco.
        private static int [] GetAsterisc(string [] splitQuery){
            
            int [] count = new int[splitQuery.Length];

            for(int i = 0; i < count.Length; i++){
                
                foreach(var ch in splitQuery[i]){
                    
                    if(ch == '*'){
                       
                        count [i]++;

                    }else{
                        
                        break;
                    }
                }
            }
            return count;
        }

        //ESte metodo respondera en caso de que se encuentre el operador "~" y lo que hara sera darnos la distancia entre
        //las dos palabras.
        public float GetDistance(string w1, string w2, Dictionary<string, int[]> wordsPosition){
            
            //float sera la variable a devolver
            //c es una constante para determinar el valor de que multiplicaremos por el score, de forma tal que la distancia divida a la constante.
            float distance = 0; 
            const float c = 10;
            
            //Verificamos si ambas palabras aparecen en el documento y si son diferentes.
            //cualquier otro caso devolveremos 1; para no afectar al score.
            if(wordsPosition.ContainsKey(w1) && wordsPosition.ContainsKey(w2) && w1 != w2){

                //Tomamos los arrays con las posiciones.
                int [] positionsW1 = wordsPosition.GetValueOrDefault(w1);
                int [] positionW2 = wordsPosition.GetValueOrDefault(w2);
                int currentDistance;
                
                //Luego iremos por las posiciones del primer array.
                for(int i = 0; i < positionsW1.Length; i++){
                    //Procedemos a recorrer las posiciones del segundo.
                    for(int k = 0; k < positionW2.Length; k++){
                        //Luego restamos las posiciones para obtener la distancia entre estas.
                        currentDistance = positionsW1[i] - positionW2[k];
                        //Si currentDistance es menor que cero, la volvemos positiva.
                        if(currentDistance < 0){
                            currentDistance = currentDistance * -1;
                        }

                        //La primera vez que entremos distance sera 0, pues es el valor con que fue inciializada. Y le daremos por valor
                        //currentDistance, ya las siguientes veces que llegemos a este punto, si encontramos una currentDistance menor
                        //que distance, entonces procedemos a cambiar distance, de forma tal que garantizamos quedarnos con la menor.
                        if(distance == 0){
                            distance = currentDistance;
                        }else if(distance > currentDistance){
                            distance = currentDistance;
                        };
                    }
                }
                //Procedemos a devolver el valor que multiplicaremos por el score, que consistira en dividir a c/distance.
                return c/distance;

            }else{
                return 1;
            }
            
        }
        public char [] GetOperator(){
            
            return this.oper;
        }
        public int GetAsteriscCount(int i){
            
            return this.countAsterisc[i];
        }
    }
}