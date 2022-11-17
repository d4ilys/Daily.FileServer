addEventListener("message",
    function(event) {
        importScripts('fileConfig.js'); //导入配置
        let param = event.data; //传过来的数据是放在参数的data属性里
        FileUp(param.file, param.chunkIndex, param.maxChunk, param.times, param.fileMd5, param.path, param.token).then(
            r => {
                if (r != undefined) {
                    postMessage({
                        $addClass: "ok",
                        $text: "上传完成",
                        $guid: param.times,
                        $removeClass: "loading",
                        $resolve: r
                    });
                }
                //关闭Worker线程
                self.close();
            });
    });

//上传文件
async function FileUp(file, chunk, maxChunk, guid, fileMd5, path, token) {
    return new Promise(function(resolve, reject) {
        var formData = new FormData();
        //将文件进行分段
        formData.append('file', file.slice(chunk * fileConfig.chunkSize, (chunk + 1) * fileConfig.chunkSize));
        formData.append('name', file.name);
        formData.append('chunk', chunk);
        formData.append('maxChunk', maxChunk);
        formData.append('guid', guid);
        formData.append('path', path);
        //发送上传请求
        var xhr = new XMLHttpRequest();
        xhr.open("POST", `${fileConfig.superfileServer}/Home/Upload/`, true);
        let process = "";
        //进度条 -- start
        var now = (chunk / maxChunk * 100).toFixed(2);
        if (now <= 100) {
            process = `${now}%`;
        }
        postMessage({
            $addClass: "loading",
            $text: process,
            $guid: guid,
            $removeClass: "",
            $resolve: ""
        });
        xhr.setRequestHeader("Authorization", token);
        //进度条 -- end
        xhr.onreadystatechange = function() {
            let data = "";
            if (xhr.readyState == 4 && xhr.status == 200) {
                data = JSON.parse(xhr.responseText);
                let result = {
                    $addClass: "",
                    $text: "",
                    $removeClass: "loading",
                    $resolve: ""
                }
                //上传完成
                if (data.msg == "ok") {
                    result.$text = "上传完成";
                    result.$addClass = "ok";
                    //获取到ID
                    resolve(data.id);
                }
                //下一个分段
                else if (data.msg == "next") {
                    //递归处理
                    FileUp(file, ++chunk, maxChunk, guid, fileMd5, path, token).then(r => {
                        resolve(r);
                    });
                } else {
                    result.$text = "上传失败";
                    result.$addClass = "error";
                }
                postMessage(result);
            }
        }
        xhr.onerror = function(er) {
            let result = {
                $addClass: "error",
                $text: "上传失败",
                $removeClass: "loading",
                $resolve: ""
            }
            postMessage(result);
        }
        xhr.send(formData);
    });
}