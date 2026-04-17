import { type SearchBlogInput } from "../types/blogs/SearchBlogInput.js";

export class BlogAPI {
    private readonly url: string;
    private readonly apiEndpoint: string = "/api/blog";
    constructor(url: string) {
        if(url.endsWith('/')) {
            this.url = url.substring(0, url.length - 1);
        } else {
            this.url = url;
        }
    }

    public SearchOnName(input: SearchBlogInput) : Promise<Response> {
        return this.BaseGetSearch("searchOnName", input);
    }

    public SearchOnContent(input: SearchBlogInput) : Promise<Response> {
        return this.BaseGetSearch("searchOnContent", input);
    }

    public Search(input?: SearchBlogInput) : Promise<Response> {
        return this.BaseGetSearch("search", input);
    }

    public SearchOnId(input: SearchBlogInput) : Promise<Response> {
        return this.BaseGetSearch("searchOnId", input);
    }

    private BaseGetSearch(endpoint: string, input?: SearchBlogInput) {
        let parmse = "?"; 
        if(input != undefined){
            parmse = this.GetRequestParms(input);
        }
        if(parmse == "?") parmse = ''
        return fetch(`${this.url}${this.apiEndpoint}/${endpoint}${parmse}`, {
            method: "GET"
        });
    }

    private GetRequestParms(input: SearchBlogInput) {
        let parms = new URLSearchParams();
        Object.entries(input).forEach(([k, v]) => {
            if(v != null) {
                parms.append(k, String(v));
            }
        });
        let parmsText = parms.toString();
        return parmsText ? `?${parms.toString()}` : '';
    }
}