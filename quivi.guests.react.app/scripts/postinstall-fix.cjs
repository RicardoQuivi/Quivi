const fs = require('fs');
const path = require('path');

const base = 'node_modules/@paybyrd/card-collect';
const patchBase = 'node_modules_patches/@paybyrd/card-collect';

fs.copyFileSync(
    path.join(patchBase, 'package.json'),
    path.join(base, 'package.json')
);