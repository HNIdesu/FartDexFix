### 使用方法：
1. 利用配套的frida脚本[https://github.com/HNIdesu/FART_Android11/](https://github.com/HNIdesu/FART_Android11/)导出dex和CodeItem文件。
2. 运行脚本输入单个dex文件和多个CodeItem文件，将会自动进行修复。
代码示例：
```
DexFixer.exe example.dex 1.json dir\2.json ...
DexFixer.exe example.dex 1.json 2.json dir\*.json dir\path\*.json ...
```