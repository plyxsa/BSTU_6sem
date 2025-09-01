import { readFileSync } from 'fs';
 import { join, basename } from 'path';

 async function runWasmInNode() {
     try {
         const wasmFilePath = join(__dirname, 'public', 'my_functions.wasm');
         const wasmBuffer = readFileSync(wasmFilePath);

         const importObject = {
         };

         const { module, instance } = await WebAssembly.instantiate(wasmBuffer, importObject);

         const x = 15;
         const y = 7;

         const sumResult = instance.exports.sum(x, y);
         const subResult = instance.exports.sub(x, y);
         const mulResult = instance.exports.mul(x, y);

         console.log(`Running WASM functions in Node.js (using ${basename(wasmFilePath)}):`);
         console.log(`sum(${x}, ${y}) = ${sumResult}`);
         console.log(`sub(${x}, ${y}) = ${subResult}`);
         console.log(`mul(${x}, ${y}) = ${mulResult}`);

     } catch (error) {
         console.error("Error running WASM in Node.js:", error);
     }
 }

 runWasmInNode();