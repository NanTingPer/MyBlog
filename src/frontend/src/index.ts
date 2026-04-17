// 创建项目: npm install -g typescript 
//          npm init typescript -y

import { BaseRequestModel } from "./types/BaseRquestModel.js";
import { BlogInfo } from "./types/blogs/BlogInfo.js";
import { BlogAPI } from "./utils/BlogAPI.js";
import { DateTimeFormat } from "./utils/DateTimeFormat.js";

// 运行项目: npm install -g http-server
//          http-server -p 80
export async function reanderBlogList(mountId: string) {
    var blogAPI = new BlogAPI("http://localhost:5162");
    let searchResponse = await blogAPI.Search();
    if(searchResponse.status != 200) {
        return;
    }

    let json = await searchResponse.json();
    let blogs = json as BaseRequestModel<BlogInfo[]>
    if(blogs.code != 200) return;
    let element = document.getElementById(mountId);
    if (element == undefined) {
        console.error(`未找到${mountId}, 无法挂载博文`);
        return;
    }
    let blogHtml = "";
    const titleClassName = "post-title";
    blogs.data?.forEach(blog => {
        let createDate = new DateTimeFormat(blog.createTime);
        // <span class="read-time">阅读时长: 2分钟</span>
        blogHtml +=  
                `<article class="blog-post">
                    <h2 class="${titleClassName}" onclick="reanderBlog('${mountId}', '${blog.id}')">${blog.name}</h2>
                    <div class="post-meta">
                        <span class="post-date">${createDate.Year()}/${createDate.Month()}/${createDate.Date()} ovo ${createDate.Hours()}:${createDate.Min()}</span>
                    </div>
                </article>`
    });
    element.innerHTML = blogHtml;
}

export async function reanderBlog(mountId: string, blogId: string) {
    let blogAPI = new BlogAPI("http://localhost:5162");
    let response = await blogAPI.SearchOnId({ keyWord: blogId });
    if (response.status != 200) return;
    let requestValue = (await response.json()) as BaseRequestModel<BlogInfo>;
    if (requestValue.code != 200 || requestValue.data == undefined) return;
    let element = document.getElementById(mountId);
    element!.innerHTML = requestValue.data.content;
}