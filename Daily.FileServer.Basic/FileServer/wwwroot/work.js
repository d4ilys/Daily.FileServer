// 子线程要用的变量
var obj = {
    md5: "nom5d",
    md5Progress:0
}

addEventListener("message", function (event) {
    importScripts('spark-md5.js'); //导入

    let file_obj = event.data, //传过来的数据是放在参数的data属性里
        fileReader = new FileReader(),
        md5 = new SparkMD5(),
        md5_sum = 0,
        currentChunk = 0,
        chunkSize = Math.ceil(file_obj.size / 100), //分成5片，每片的大小
        start = 0;  //起始字节
    let loadFile = () => {
        let slice = file_obj.slice(start, start + chunkSize);  //根据字节范围切割每一片
        fileReader.readAsBinaryString(slice);
    }
    loadFile();
    fileReader.onload = e => {
        md5.appendBinary(e.target.result);
        currentChunk++;
        if (start < file_obj.size) {
            let chunkMd5 = parseInt(start / file_obj.size * 100);
            obj.md5Progress  = chunkMd5;
            postMessage(JSON.stringify(obj));
            start += chunkSize;
            loadFile();
        } else {
            md5_sum = md5.end();
            obj.md5 = md5_sum;
            postMessage(JSON.stringify(obj));
        }
    };
});
