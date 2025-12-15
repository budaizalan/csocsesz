const express = require('express');

class UserRouter {
    constructor(userController) {
        if(!userController) {
            throw new Error('UserController is required to initialize UserRouter');
        }
        this.userController = userController;
        this.router = express.Router();
        this.initializeRoutes();
    }
    initializeRoutes() {
        this.router.get('/', this.userController.getAllUsers.bind(this.userController));
        this.router.get('/:id', this.userController.getUserById.bind(this.userController));
        this.router.post('/', this.userController.createUser.bind(this.userController));
        this.router.delete('/:id', this.userController.deleteUser.bind(this.userController));
    }
    getRouter() {
        return this.router;
    }
}
module.exports = UserRouter;