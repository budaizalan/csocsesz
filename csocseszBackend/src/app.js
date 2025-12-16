const express = require('express');
const cors = require('cors');
const UserRouter = require('./routes/userRouter');
const mongoose = require('mongoose');
const UserController = require('./controllers/userController');
const MatchController = require('./controllers/matchController');
const MatchRouter = require('./routes/matchRoutes');
require('./models/Match');
require('./models/User');
const dotenv = require('dotenv');
dotenv.config({ path: require("path").resolve(__dirname, ".env") });
const app = express();
const port = process.env.PORT || 5000;
app.use(cors());
app.use(express.json());

mongoose.connect(process.env.MONGODB_URI, { dbName: 'csocseszDB', maxPoolSize: 20 })
    .then(() => console.log('Connected to MongoDB'))
    .catch((err) => {
        console.error('MongoDB connection error:', err);
        process.exit(1);
    });
mongoose.connection.on('error', (err) => {
    console.error('MongoDB connection error:', err);
});

const userRouter = new UserRouter(UserController);
const matchRouter = new MatchRouter(MatchController);

app.setMaxListeners(15);

app.use('/api/reset', (req, res) => {UserController.resetUsersStats(req, res); MatchController.deleteMatches(req, res);});
app.use('/api/users', userRouter.getRouter());
app.use('/api/matches', matchRouter.getRouter());

app.listen(port, () => {
    console.log(`Server is running on port: ${port}`);
});
module.exports = app;