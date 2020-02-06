'use strict';
var express = require('express');
var router = express.Router();

/* GET users listing. */
router.get('/', function (req, res) {
    res.render('users');
    console.log(req.app.locals.typeOfAuthenticated);
});

module.exports = router;
