const express = require('express');
const path = require('path');
const fs = require('fs');
const { spawn } = require('child_process');

const app = express();

app.use(express.static(path.join(__dirname, '/dist')));

app.post('/artillery', (req, res, next) => {
  const ls = spawn('artillery', ['run', '--insecure', '--output', './results.json', 'artillery.yml'] );

  ls.stdout.on('data', data => {
    console.log(`stdout: ${data}`);
  });

  ls.stderr.on('data', data => {
    console.log(`stderr: ${data}`);
  });

  ls.on('close', code => {
    console.log(`child process exited with code ${code}`);
  });

  next();
});

app.get('/artillery', (req, res, next) => {
  const readStream = fs.createReadStream(path.resolve(__dirname, 'results.json'));
  let chunks = [];

  readStream.on('error', err => {
    res.end();
  })

  readStream.on('data', chunk => {
    chunks.push(chunk);
  });

  readStream.on('close', () => {
    const result = JSON.parse(Buffer.concat(chunks).toString());
    res.status(200).send(result);
  });

});

app.get('*', (req, res, next) => {
  res.sendFile(path.resolve(path.join(__dirname, '/dist'), 'index.html'));
});

const port = process.env.PORT || 4000;
console.log(`App listening on port ${port}`)
app.listen(port);