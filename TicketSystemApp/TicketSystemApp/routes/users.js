'use strict';
var express = require('express');
var router = express.Router();
var XMLHttpRequest = require("xmlhttprequest").XMLHttpRequest;
var request = require('request');

/* GET users listing. */
router.get('/:id', function (req, res) {

    var customer;
    var customerTickets;
    var coupons;

    async function PopulateCustomer() {
        customer = await GetCustomer();
        coupons = await GetCustomerCoupons();
        customerTickets = await PopulateCustomerTickets();
    }

    async function PopulateCustomerTickets() {
        customerTickets = await GetCustomerTickets();
        res.render('users', { cust: customer, customerTickets, customerTickets, coupons: coupons });
    }

    PopulateCustomer();

    function GetCustomer() {
        return new Promise(resolve => {
            var xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function () {
                if (this.readyState == 4 && this.status == 200) {
                    resolve(JSON.parse(this.responseText));
                }
            }

            xhr.open("GET", "http://127.0.0.10/api/Customers/" + req.params.id, true);
            xhr.send();
        });
    }

    function GetCustomerTickets() {
        return new Promise(resolve => {
            var xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function () {
                if (this.readyState == 4 && this.status == 200) {
                    resolve(JSON.parse(this.responseText));
                }
            }

            xhr.open("GET", "http://127.0.0.10/api/Customers/" + req.params.id + "/Tickets", true);
            xhr.send();
        });
    }

    function GetCustomerCoupons() {
        return new Promise(resolve => {
            var xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function () {
                if (this.readyState == 4 && this.status == 200) {
                    resolve(JSON.parse(this.responseText));
                }
            }
            xhr.open("GET", "http://127.0.0.10/api/Customers/" + req.params.id + "/Coupons", true);
            xhr.send();
        });
    }

});

router.post('/addFunds', function (req, res) {

    request.post("http://127.0.0.10/api/Customers/AddFunds", {
        json: {
            CustomerId: req.app.locals.globalCustomer.customerId,
            Currency: parseFloat(req.body.amount)
        }
    }, (error, response, body) => {
            if (!error && response.statusCode == 200) {
                res.redirect("/users/" + req.app.locals.globalCustomer.customerId);
            }
    })

});

module.exports = router;
