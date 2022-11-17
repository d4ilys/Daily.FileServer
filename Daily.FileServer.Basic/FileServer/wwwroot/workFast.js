// 子线程要用的变量
var obj = {
    md5: "nom5d",
    md5Progress: 0
}
const chunkSize = 64 * 1024 * 1024;
const fileReader = new FileReader();
let hasher = null;
addEventListener("message",
    async (event) => {
        importScripts('hash-wasm.js'); //导入
        let file = event.data; //传过来的数据是放在参数的data属性里
        const readFile = async (file) => {
            if (hasher) {
                hasher.init();
            } else {
                hasher = await hashwasm.createMD5();
            }

            const chunkNumber = Math.floor(file.size / chunkSize);
            for (let i = 0; i <= chunkNumber; i++) {
                console.log()
                obj.md5Progress = chunkNumber == 0 ? 0 : parseInt(i / chunkNumber * 100);
                postMessage(JSON.stringify(obj));
                const chunk = file.slice(
                    chunkSize * i,
                    Math.min(chunkSize * (i + 1), file.size)
                );
                await hashChunk(chunk);
            }
         
            const hash = hasher.digest();
            return Promise.resolve(hash);
        };
        const md5Res = await readFile(file);
        obj.md5 = md5Res;
        obj.md5Progress = 100;
        postMessage(JSON.stringify(obj));
        //关闭Worker线程
        self.close();
    });

function hashChunk(chunk) {
    return new Promise((resolve, reject) => {
        fileReader.onload = async (e) => {
            const view = new Uint8Array(e.target.result);
            hasher.update(view);
            resolve();
        };

        fileReader.readAsArrayBuffer(chunk);
    });
}