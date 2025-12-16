const express = require('express');

class MatchRouter {
    constructor(matchController) {
        if(!matchController) {
            throw new Error('MatchController is required to initialize MatchRouter');
        }
        this.matchController = matchController;
        this.router = express.Router();
        this.initializeRoutes();
    }
    initializeRoutes() {
        this.router.get('/', this.matchController.getAllMatches.bind(this.matchController));
        this.router.post('/', this.matchController.postMatch.bind(this.matchController));
        this.router.delete('/', this.matchController.deleteMatches.bind(this.matchController));
    }
    getRouter() {
        return this.router;
    }
}

module.exports = MatchRouter;