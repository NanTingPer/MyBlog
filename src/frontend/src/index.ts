// 创建项目: npm install -g typescript 
//          npm init typescript -y

import { BaseRequestModel } from "./types/BaseRquestModel.js";
import { BlogInfo } from "./types/blogs/BlogInfo.js";
import { BlogAPI } from "./utils/BlogAPI.js";

// 运行项目: npm install -g http-server
//          http-server -p 80
export async function sayHelloWorld(world: string) {
    let _docuemnt = document.createElement('a');
    _docuemnt.innerHTML = "恭喜加载了";
    var blogAPI = new BlogAPI("http://localhost:5162");
    let searchResponse = await blogAPI.Search();
    if(searchResponse.status == 200) {
        let json = await searchResponse.json();
        let blogs = json as BaseRequestModel<BlogInfo[]>
        console.log(blogs);
    }
    return `Hello ${world}`;
}
