//const Koa = require('koa');
//const app = new Koa();
//
//app.use(async ctx => {
//	// 允许来自所有域名请求
//    ctx.set("Access-Control-Allow-Origin", "*");
//	// 设置所允许的HTTP请求方法
//    ctx.set("Access-Control-Allow-Methods", "OPTIONS, GET, POST");
//	// 字段是必需的。它也是一个逗号分隔的字符串，表明服务器支持的所有头信息字段.
//    ctx.set("Access-Control-Allow-Headers", "x-requested-with, accept, origin, content-type");
//	console.log(ctx.request);
//    ctx.body = 'Hello Vercel';
//});
//
//app.listen(30000, () => {
//    console.log('30000 nodejs请求转发项目启动')
//});

var http = require('http');

const REDIRECT = "redirect";
const PORT = 30000;

function param(url) {
	let __param = [];
	if (url) {
		const index = url.indexOf('?');
		if (index == -1)
			return __param;
		var array = url.substring(index + 1).split("&");
		for (var item of array){
			var split = item.split("=");
			__param[split[0]] = decodeURIComponent(split[1]);
		}
	}
	return __param;
}
function get(param) {
	let __result = "";
	for (const key of param) {
		if (key == REDIRECT)
			continue;
		__result += "&" + key + "=" + encodeURIComponent(param[key]);
	}
	if (__result.length)
		__result = "?" + __result.substring(1);
	return __result;
}

http.createServer(function (request, response) {
	// 转发http请求
	// 从url中获取redirect参数，代表实际的目标接口url，例如https://ee-pass.vercel.com/?redirect=http://127.0.0.1/
	// 然后向解析出来的地址将request的ContentType,Timeout,Header,Cookie,Body转发
	// 将返回内容的StatusCode,StatusMessage,Header,Body发回给response即可
	const { headers, method, url } = request;
	let params = param(url);
	let redirect = params[REDIRECT];
	if (!redirect) {
		response.setHeader("access-control-allow-origin", "*");
		response.setHeader("access-control-allow-methods", "POST,GET,OPTIONS");
		if (headers["access-control-request-headers"])
			response.setHeader("access-control-allow-headers", headers["access-control-request-headers"]);
		response.writeHead(200, { "Content-Type": "text/plain;charset=utf-8" });
		response.end("请携带encodeURIComponent后的redirect参数向此地址请求转发");
		return;
	}

	let body = [];
    request.on('error', (err) => {
		response.setHeader("access-control-allow-origin", "*");
		response.writeHead(500, { "Content-Type": "text/plain;charset=utf-8" });
		response.end("服务器错误");
    }).on('data', (chunk) => {
		body.push(chunk);
    }).on('end', () => {
		body = Buffer.concat(body);
		const options = {
			method: method,
			headers: headers,
		};
		const __body = body;
		const req = http.request(redirect + get(params), options, res => {
			response.writeHead(res.statusCode, res.statusMessage, res.headers);
			body = [];
			
			res.on('data', (chunk) => {
				body.push(chunk);
			});
			res.on('end', () => {
				body = Buffer.concat(body);
				response.end(body);
			});
		});
		req.on('error', (err) => {
			response.setHeader("access-control-allow-origin", "*");
			response.writeHead(500, { "Content-Type": "text/plain;charset=utf-8" });
			response.end("服务器错误");
		});
		if (method == "POST" && __body.length) {
			//console.log("transfer body: ", body);
			req.write(__body);
		}
		req.end();
    });
}).listen(PORT);

console.log("跳转服务器启动成功！端口：", PORT);