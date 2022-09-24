namespace MoogleEngine;


public static class Moogle
{   
    public static Data data;
    public static DataTFIDF vTFIDF;
    public static Suggestion sgt;
    static bool [] matchQuery;

    public static SearchResult Query(string query) {
        
        /*Normalizamos la query.*/
        string [] splitQuery = Data.NormalizeText(query).Split();
        string [] unSplitQuery = query.Split();
                            
        /*Verificamos si tenemos algun operador en la query*/
        Operator oper = new Operator(unSplitQuery);
                             
        /*Puntuamos cada documento de acuerdo a la query.(GetScore)*/
        float [] docScore = GetScore(data.GetNames().Length, splitQuery, vTFIDF.GetTFIDF(), oper);
                                    
        /*Ordenamos los documentos por score.(GetScorePosition)*/
        KeyValuePair<float, int>[] ScorePosition = GetScorePosition(docScore);
                                           
        /*Counter sera para definir cuantos documentos contienen al menos una query.*/
        int counter = 0;                  
                                              
        /*Recorremos los scores, si encontramos uno que sea cero, de ahi en adelante todos lo seran y 
        se detendra el conteo.*/
        while(ScorePosition.Length > counter){
           
            if(ScorePosition[counter].Key != 0){
                counter++;
            }else{
                break;
            }
        }
                                           
        /*Ejecutamos el metodo FinalResult, que con toda la informacion anterior, dara el resultado a la query.*/
        return FinalResult(query, counter, data.GetNames(), ScorePosition, unSplitQuery, splitQuery, oper);
    }                                       
    
  
    /*Este metodo(GetScore) le da una puntuacion a cada documento en funcion de la query.*/
    private static float [] GetScore(int fileNumber, string [] splitQuery, Dictionary<string, float>[] TFIDF, Operator oper){
        //Inicializamos wordsPositions.
        Dictionary<string, int[]> [] wordsPosition = data.GetWordsPosition(); 

        /*Si alguna palabra de la query no aparece nunca, tendra un valor de false.*/
        matchQuery = new bool[splitQuery.Length];
        
        /*Cargamos los operadores para su verificacion.*/
        char [] operQuery = oper.GetOperator();

        /*docScore es el array que devolvere, tendra la puntuacion de cada documento para la query.*/
        float[]docScore = new float[fileNumber];

        float distanceValue = 0;

        /*Un bucle for para ir por cada documento.*/
        for(int i = 0; i < fileNumber; i ++){ 

            /*A continuacion iremos viendo cada palabra de la query en el documento i.*/
            for(int j = 0; j < splitQuery.Length; j++){
                //Verificamos que la palabra aparezca en el documento y que no sea vacia.
                //En caso de que la palabra no aparezca, verificamos si no es porque era un operador de cercania.
                //Y si no aparece y no es un operador de cercania y la palabra tiene el operador ^, procedemos a darle
                //0 de score al documento y break;
                if((TFIDF[i].ContainsKey(splitQuery[j]) && splitQuery[j] != string.Empty)){
                    //Procedemos a verificar los operadores.
                    if(operQuery[j] == '*'){
                        //Calculamos la cantidad de asteriscos con GetAsteriscCount() y lo multiplicamos por el score de la palabra
                        //y lo acumulamos como socre del documento.
                        int count = oper.GetAsteriscCount(j);
                        docScore[i] += (TFIDF[i].GetValueOrDefault(splitQuery[j]) * count);

                    }else if(operQuery[j] == '!'){
                        //En caso de que encontremos el operador !, pues le damos 0 de score al documento pues no debe
                        //ser devuelto, marcamos matchQuery como false y procedemos a break;
                        docScore[i] = 0;
                       
                        break;
                    }else{
                        docScore[i] += TFIDF[i].GetValueOrDefault(splitQuery[j]);
                    }
                    
                    matchQuery[j] = true;
               
                }else if(operQuery[j] == '~'){
                    //Calculamos el valor que multiplicaremos por el score final del documento mas adelante.                       
                    distanceValue = oper.GetDistance(splitQuery[j-1], splitQuery[j+1], wordsPosition[i]);

                }else if(operQuery[j] == '^'){
                    
                    docScore[i] = 0;
                    
                    break;
                }
            }
            //Si distanceValue es mayor que cero, pues multiplicamos todo el score del documento por ella.
            if(distanceValue > 0){
                docScore[i] = docScore[i] * distanceValue;
            }
        }

        return docScore;
    }

    /*Este metodo(GetScorePosition) ordena los documentos por score.*/
    private static KeyValuePair<float, int>[] GetScorePosition(float[] docScore){

        /*Se utilizara un KeyValuePair, donde key=score y value=position.*/
        KeyValuePair<float, int> [] ScorePosition = new KeyValuePair<float, int>[docScore.Length];
        
        /*A continuacion iremos asociandole a cada score su posicion.*/
        for(int i = 0; i < docScore.Length; i++) {
            
            ScorePosition[i] = new KeyValuePair<float, int>(docScore[i], i);
        }
        
        /*Ordenamos por score, acto seguido se devuelve.*/
        return ScorePosition = ScorePosition.OrderByDescending(x=>x.Key).ToArray();
    }

    //Este metodo devuelve un array de string, donde cada posicion es el mejor snnipet de cada documento en funcion de la query.
    private static string [] GetFinalSnnipets(Data data, DataTFIDF vTFIDF, string [] normSplitQuery, KeyValuePair<float, int>[] scorePosition, Operator oper){
        //Para los snnipets necesitamos el valor de de TFIDF de cada palabra.
        //Tambien obtendremos la variable snnipets
        //y ademas finalSnnipets es donde devolveremos los mejores snnipets para cada documento.
        var TFIDF = vTFIDF.GetTFIDF();
        var snnipets = data.GetSnnipets();
        string [] finalSnnipets = new string [TFIDF.Length];
        var operQuery = oper.GetOperator();
       
       //Procedeomos a ir por cada score en scorePosition, lo que equivale a ir por cada documento
       //y como en score position estan ordenados de mayor a menor por su score, en cuanto encontremos
       //uno que sea cero, el resto seran ceros.
        foreach(var score in scorePosition){

            if(score.Key > 0){
                //Declaramos string bestSnnipet, que sera el mejor snnipet del documento
                //que estemos trabajando y el bestScore, sera el score de dicho snnipet.
                string bestSnnipet = "";
                float bestScore = 0;
                //luego iremos por cada linea del documeno al que le corresponde el score
                foreach(var line in snnipets[score.Value]){
                    //Declaramos currentScore, la idea sera irnos quedando con el snnipet que saque mejor score.
                    float currentScore = 0;

                    string[] currentLine = line.Value.Split();

                    //Para el calculo del score de los snnipets, iremos viendo por cada palabra de la linea
                    //si cada palabra de la query se encuentra en la lina, ademas verificaremos el operador de importancia.
                    for(int i = 0; i < currentLine.Length; i++){
                        
                        for(int j = 0; j < normSplitQuery.Length; j++){
                            
                            if(currentLine[i] == normSplitQuery[j] && normSplitQuery[j] != string.Empty && TFIDF[score.Value].ContainsKey(normSplitQuery[j])){
                                
                                if(operQuery[j] == '*'){
                                    currentScore += TFIDF[score.Value].GetValueOrDefault(normSplitQuery[j]) * oper.GetAsteriscCount(j);                  
                                }else{
                                    currentScore += TFIDF[score.Value].GetValueOrDefault(normSplitQuery[j]);
                                }
                            }
                        }
                        
                        
                    }
                    if(bestScore < currentScore){
                        bestScore = currentScore;
                        bestSnnipet = line.Key;
                    }
                }
                
                finalSnnipets[score.Value] = bestSnnipet;
            }else{
                break;
            }
        }
        return finalSnnipets;
    }
    private static SearchResult FinalResult(string query, int counter, string [] fileName, KeyValuePair<float, int> [] ScorePosition, string [] unSplitQuery, string[] splitQuery, Operator oper){

        /*Declaro el array de tipo SearchItem item.*/
        SearchItem[] items;
        
        /*Si el counter es mayor que cero, entonces significa que hay al menos 1 documento como respuesta a la query.*/
        if(counter > 0){
            
            /*Determinamos los snnipets para cada documento que es respuesta a la query y lo almacenamos en un array, cuya
            posicion coincidira con la del documento en cualquier otra estructura, por ejemplo con las posiciones asociadas
            en scorePosicion.*/
            string [] snnipets = GetFinalSnnipets(data, vTFIDF, splitQuery, ScorePosition, oper);
            
            /*Inicializamos items con una longitud de "counter", pues es la cantidad de archivos a devolver.*/
            items = new SearchItem[counter];
            
            /*A continuacion iremos llenando items con los nombres, los snnipets y el score de cada documento que respondio
            a la query.*/
            for(int i = 0; i < items.Length; i++){            
                
                items[i] = new SearchItem(fileName[ScorePosition[i].Value], snnipets[ScorePosition[i].Value], ScorePosition[i].Key);
            }

            /*Procedemos a verificar si existe alguna palabra de la query que no tenga respuesta 
            en los documentos.
            Declaro la variable bool existSuggestion y la inicializo con un valor de false.*/
            bool existSuggestion = false;
            string suggestion = "";
            
            /*A continuacion, recorremos matchQuery.*/
            for(int i = 0; i < matchQuery.Length; i++){
                                 
                /*Si es false, buscaremos una sugerencia.*/
                if(matchQuery[i] == false){

                    string temp = sgt.GetSuggestion(splitQuery[i]);
                    
                    /*Si la sugerencia no es empty(existSuggestion se vuelve true) y no estamos al final, agregamos temp y un espacio a suggestion.
                    Si la sugerencia es empty y no estamos al final, agregamos unSplitQuery en la posicion i y un 
                    espacio a suggestion.*/
                    
                    if(temp != string.Empty && (i != matchQuery.Length - 1)){
                      
                        suggestion += temp + " ";
                        existSuggestion = true;

                    }else if(temp != string.Empty){
                        
                        suggestion += temp;
                        existSuggestion = true;

                    }else if(i != matchQuery.Length - 1){
                        
                        suggestion += unSplitQuery[i] + " ";
                    }else{
                       
                        suggestion += unSplitQuery[i];
                    }            
                }else{
                    /*En caso de que si aparezca, solamente acoplamos a la sugerencia la palabra.*/
                    if(i != matchQuery.Length - 1){
                        suggestion += unSplitQuery[i] + " ";
                    }else{
                        suggestion += unSplitQuery[i];
                    }
                }
            }
            
            /*Si existe la sugerencia, se llama al constructor adecuado de la clase SearchResult.*/
            if(existSuggestion == true){
                return new SearchResult(items, suggestion);
            }else{
                return new SearchResult(items);
            }

        /*Si el contador es cero, pero la query no esta vacia, trataremos de encontar una sugerencia adecuada.*/
        }else if(counter == 0 && query != string.Empty){
            
            string suggestion = "";
            string temp;
            
            for(int i = 0; i < splitQuery.Length; i++){
                temp = sgt.GetSuggestion(splitQuery[i]);
                
                if(temp != string.Empty &&(i != splitQuery.Length - 1)){
                    suggestion += temp + " ";
                }else if(temp != string.Empty){
                    suggestion += sgt.GetSuggestion(splitQuery[i]);
                }
            }
            /*Si la sugerencia no es empty, la devolvemos, sino, no devolveriamos nada.*/
            if(suggestion != string.Empty){
                return new SearchResult(suggestion);
            }else{
                return new SearchResult();
            }
        
        /*Si la query esta vacia y counter = 0, no hay que devolver nada.*/
        }else{
            return new SearchResult();
        }
    }
}
