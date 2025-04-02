define([
    'jquery',
    'angular',
    'underscore',
    'url'
],
    function ($,angular, _, url) {
        'use strict';

        var settingsSources = [
            'Blue.Config.ContactType',
            'Blue.Cosacs.Merchandising.VendorTags',
            'Blue.Cosacs.Merchandising.VendorCurrencies'
        ];

        var importContactTypes = [
            'Contact Name',
            'Contact Email',
            'Contact Phone',
            'Company Phone'
        ];

        return function ($scope, $location, pageHelper, user, vendorResourceProvider) {

            pageHelper.getSettings(settingsSources, function (options) {
                function insertImportContactTypes() {
                    _.each(importContactTypes, function (type) {
                        if (_.indexOf($scope.options.contactType, type) < 0) {
                            $scope.options.contactType.push(type);
                        }
                    });
                }

                $scope.options = options;
                insertImportContactTypes();
                $scope.$apply();
            });

            $scope.updateContact = function (contact) {
                if (!contact.key || contact.key.length === 0) {
                    contact.value = '';
                }
            };

            $scope.types = ["Local", "CARICOM", "International"];
            $scope.selectedStatus = {};
            $scope.statuses = [];

            $scope.readonly = !user.hasPermission('VendorEdit');
            //---------------Product Import Validation----------------
            function alert(text, title) {
                var $c = $('#confirm').modal({
                    show: true
                })
                $c.find('button.cancel').hide();
                $c.find('h3').html(title);
                $c.find('p').html(text || '');
            }
            $('#confirm').find('button.ok').off('click').on('click', function () {
                return $('#confirm').modal('hide');
            });
            function validateComma(str) {
                if (str === null || str === undefined)//if null or undefined skip validation
                    return true;
                if (str.indexOf(',') > -1) {
                    return false; //if the str contains the comma then reutn false
                }
                return true;//otherwise return true
            }
            //---------------Product Import Validation End----------------

            $scope.save = function () {
                //
                var failedValidationCount = 0;
                var failedValidationMessage = "<b> Please correct below details - </b><br />";

                if (!validateComma($scope.vm.supplier.name)) {
                    failedValidationCount++;
                    failedValidationMessage += "<br />" + failedValidationCount.toString() + ". Remove comma from field <b>\"Vendor Name\"</b>";
                }
                if ($scope.vm.supplier.name.length > 40) {
                    failedValidationCount++;
                    failedValidationMessage += "<br />" + failedValidationCount.toString() + ". <b>\"Vendor Name\"</b> Exceeds its max legth i.e. 40 characters";
                }
                if (failedValidationCount > 0) {
                    return alert(failedValidationMessage, "Field Validation");
                    return;
                }
                var stat = _.find($scope.vm.statuses, function (item) {
                    return item.id === $scope.selectedStatus;
                });

                $scope.vm.supplier.status = stat.id;
                $scope.vm.supplier.tags = $scope.supplierTags;
                vendorResourceProvider
                    .save($scope.vm.supplier)
                    .then(function (response) {
                        $scope.vm.supplier.id = response.supplier.id;
                        pageHelper.setTitle('Vendor - ' + $scope.vm.supplier.name);
                        $location.path(url.resolve('Merchandising/Vendors/Detail/' + response.supplier.id));
                        pageHelper.notification.show('Vendor saved successfully.');
                    }, function (response) {
                        pageHelper.notification.showPersistent(response.message);
                    });
            };

            $scope.supplierTags = [];
         
            $scope.$watch('vm', function (vm) {
                $scope.supplierTags = vm.supplier.tags;
                
                $scope.statuses = vm.statuses;
                if (vm.supplier !== null && vm.supplier.id > 0) {
                    var match = _.where($scope.statuses, { id: vm.supplier.status });
                    $scope.selectedStatus = match[0].id;
                } else {
                    vm.supplier = {};
                    $scope.selectedStatus = $scope.statuses[0].id;
                }
               
                vm.supplier.contacts = vm.supplier.contacts || [];
                if (vm.supplier.contacts.length < 1) {
                    $scope.addContact();
                }
            });

            $scope.saveVendorTags = function () {
                if ($scope.vm.supplier !== null) {
                    $scope.saving = true;
                    vendorResourceProvider.saveVendorTags($scope.vm.supplier.id, $scope.supplierTags).then(function () {
                        pageHelper.notification.show('Vendor tag saved successfully.');
                        $scope.saving = false;
                    }, function () {
                        $scope.saving = false;
                    });
                }
            };

            $scope.vendorExists = function () {
                return $scope.vm.supplier && $scope.vm.supplier.id;
            };

            $scope.addContact = function () {
                $scope.vm.supplier.contacts.push({
                    type: 'Email',
                    value: ''
                });
            };

            $scope.removeContact = function (contact) {
                $scope.vm.supplier.contacts = _.reject($scope.vm.supplier.contacts, function (c) { return c === contact; });
            };
        };
    });
