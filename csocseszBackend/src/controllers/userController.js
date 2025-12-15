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