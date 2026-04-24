const fs = require('fs/promises');
const path = require('path');

const jssrc = path.join(__dirname, 'src/assets/js');   // 源目录
const jsdist = path.join(__dirname, 'dist/assets/js'); // 目标目录

const csssrc = path.join(__dirname, 'src/assets/css');   // 源目录
const cssdist = path.join(__dirname, 'dist/assets/css'); // 目标目录


async function copyDirectory() {
  try {
    await fs.cp(jssrc, jsdist, { recursive: true, force: true });
    await fs.cp(csssrc, cssdist, { recursive: true, force: true });
  } catch (err) {
  }
}

copyDirectory();