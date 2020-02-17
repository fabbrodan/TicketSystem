'use strict';
var express = require('express');
var router = express.Router();
var XMLHttpRequest = require("xmlhttprequest").XMLHttpRequest;

/* GET users listing. */
router.get('/:id', function (req, res) {

    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {

            res.render('users', { customer: JSON.stringify(this.responseText) })
        }
    }

    xhr.open("GET", "http://127.0.0.10/api/Customers/" + req.params.id, true);
    xhr.send();
});

module.exports = router;
