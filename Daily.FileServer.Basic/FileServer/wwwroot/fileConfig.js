
//上传设置
var fileConfig = {
    //生成唯一标识码
    guid: (function () {
        var counter = 0;

        return function (prefix) {
            var guid = (+new Date()).toString(32),
                i = 0;

            for (; i < 5; i++) {
                guid += Math.floor(Math.random() * 65535).toString(32);
            }

            return (prefix || 'wu_') + guid + (counter++).toString(32);
        };
    })(),
    chunkSize: 1024 * 1024 * 3, //每次上传的字节大小  默认3M
    superfileServer: ``,
    token:''  
};

function getToken(url,baseToken) {
    //发送上传请求
    let xhr = new XMLHttpRequest();
    xhr.open("Get", `${url}/SafetyAndMaintenance/Education/GetToken`, true);
    xhr.setRequestHeader("Authorization", baseToken);
    xhr.onreadystatechange = function () {
        if (xhr.readyState == 4 && xhr.status == 200) {
            fileConfig.token = `Bearer ${xhr.responseText}`;
        }
    }
    xhr.send();
}

