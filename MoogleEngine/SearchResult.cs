namespace MoogleEngine;

public class SearchResult
{
    private SearchItem[] items;

     public SearchResult(string suggestion = ""){
        this.items = new SearchItem[1];
        this.items[0] = new SearchItem("Lo sentimos", "No hemos encontrado nada", 0);
        this.Suggestion = suggestion;
    }
    public SearchResult(SearchItem[] items){

        if(items == null) {
            throw new ArgumentNullException("items");
        }

        this.items = items;
    }
    public SearchResult(SearchItem[] items, string suggestion="")
    {
        if (items == null) {
            throw new ArgumentNullException("items");
        }

        this.items = items;
        this.Suggestion = suggestion;
    }

    public SearchResult() : this(new SearchItem[0]) {

    }

    public string? Suggestion { get; private set; }

    public IEnumerable<SearchItem> Items() {
        return this.items;
    }

    public int Count { get { return this.items.Length; } }
}
