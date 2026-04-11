// 创建项目: npm install -g typescript 
//          npm init typescript -y

// 运行项目: npm install -g http-server
//          http-server -p 80
export function sayHelloWorld(world: string) {
    let _docuemnt = document.createElement('a');
    _docuemnt.innerHTML = "恭喜加载了";
  return `Hello ${world}`;
}
