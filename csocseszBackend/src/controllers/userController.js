const mongoose = require('mongoose');

class UserController{
    async getAllUsers(req, res){
        try {
            const users = await mongoose.model('User').find();
            res.status(200).json(users);
        } catch (error) {
            res.status(500).json({ error:  error.message });
        }
    }

    async getUserById(req, res){
        try {
            const user = await mongoose.model('User').findById(req.params.id);
            if(!user){
                return res.status(404).json({ error: 'User not found' });
            }
            res.status(200).json(user);
        } catch (error) {
            res.status(500).json({ error: 'Internal Server Error' });
        }
    }

    async resetUsersStats(req, res){
        try {
            const User = await mongoose.model('User');
            if(req.query && req.query.pushupOnly){
                console.log("Reset pushups");
                
                const result = await User.updateMany({}, {
                    $set: {
                        'stats.totalPushUps': 0
                    }
                });
                return res.status(200).json({ message: 'User push-up stats reset successfully', modifiedCount: result.nModified });
            }
            const result = await User.updateMany({}, {
                $set: {
                    'stats.streak': 0,
                    'stats.totalMatchWon': 0,
                    'stats.totalMatchLost': 0,
                    'stats.totalPushUps': 0
                }
            });
            res.status(200).json({ message: 'User stats reset successfully', modifiedCount: result.nModified });
        } catch (error) {
            res.status(500).json({ error: 'Internal Server Error' });
        }
    }

    async createUser(req, res){
        try {
            const newUser = new (mongoose.model('User'))(req.body);
            await newUser.save();
            res.status(201).json(newUser);
        } catch (error) {
            res.status(500).json({ error: error.message });
        }
    }

    async deleteUser(req, res){
        try {
            const deletedUser = await mongoose.model('User').findByIdAndDelete(req.params.id);
            if(!deletedUser){
                return res.status(404).json({ error: 'User not found' });
            }
            res.status(200).json({ message: 'User deleted successfully' });
        } catch (error) {
            res.status(500).json({ error: 'Internal Server Error' });
        }
    }
}

module.exports = new UserController();