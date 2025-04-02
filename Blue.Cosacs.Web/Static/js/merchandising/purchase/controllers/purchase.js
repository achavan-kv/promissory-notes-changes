define([
    'angular',
    'underscore',
    'url',
    'confirm',
    'moment',
    'lib/select2',
    'jquery',
    'datepicker'
],
    function (angular, _, url, confirm, moment) {
        'use strict';

        var settingsSources = [
            'Blue.Cosacs.Merchandising.ReferenceNumberType'
        ];

        return function ($scope, $timeout, $location, pageHelper, user, purchaseResourceProvider, vendorResourceProvider, locationResourceProvider, additionalCostProvider) {
            var beforeEditClone = {};

            $scope.editing = false;
            $scope.editingLabels = false;
            $scope.saving = false;
            $scope.userCanEdit = !user.hasPermission('PurchaseOrderEdit');
            $scope.dateFormat = pageHelper.dateFormat;
            $scope.today = moment().format('YYYY-MM-DD');
            $scope.ApproveRejectOrderPermissionFirst = user.hasPermission('AshleyPOFirstApproveReject');
            $scope.ApproveRejectOrderPermissionSecond = user.hasPermission('AshleyPOSecondApproveReject');
            $scope.Hideshowbutton = true;

            console.log($scope.ApproveRejectOrderPermissionFirst, $scope.ApproveRejectOrderPermissionSecond);

            $scope.$watch('purchase', function (purchase) {
                updateMode(purchase);
                updateTitle(purchase || {});
                initializeEditMode();
                initializeReferenceNumbers(purchase);
                $scope.partitionedAttributeData = pageHelper.partition($scope.purchase.attributes, 3);
            });

            //function updateMinDate() {
            //    $timeout(function() {
            //        $('#requestedDeliveryDate').datepicker('option', 'minDate', new Date());
            //    },0);
            //}

            function updateMode(purchase) {
                if (!purchase.id) {
                    $scope.mode = 'create';
                    $scope.readonly = false;
                } else if (purchase.status === 'Completed' || purchase.status === 'Cancelled' || purchase.status === 'Approved' || purchase.status === 'Rejected') {
                    $scope.mode = 'view';
                    if ($scope.purchase.cancelReasonId) {
                        $scope.purchase.cancelReason = $scope.POCancelReasonList[parseInt($scope.purchase.cancelReasonId)];
                    }
                    $scope.readonly = true;
                }
                else {
                    $scope.mode = 'edit';
                    $scope.readonly = $scope.userCanEdit;
                }
            }

            function isCreateMode() {
                return $scope.mode === 'create';
            }

            function isEditMode() {
                return $scope.mode === 'edit';
            }

            function isViewMode() {

                // Code Added by Abhijeet Gawali for Ashley CR:-  
                // Hide Approve Button if PO Status is PartiallyApproved
                if ($scope.purchase.status === 'PartiallyApproved' && $scope.ApproveRejectOrderPermissionFirst === true && $scope.ApproveRejectOrderPermissionSecond === false) {
                    $scope.Hideshowbutton = false;
                }
                // End Code
                return $scope.mode === 'view';
            }

            function canAddProducts() {
                return !$scope.readonly && $scope.mode === 'create' && !$scope.saving && typeof $scope.purchase.vendorId !== 'undefined';
            }

            function canSave(form) {
                return !$scope.readonly && $scope.editing && !$scope.saving && $scope.purchase.products.length > 0 && !$scope.editingProduct && form.$valid;
            }

            function canCancel() {
                return !$scope.readonly && $scope.mode === 'edit' && $scope.editing && !$scope.saving;
            }

            function canCancelOrder() {
                return !$scope.readonly && $scope.mode === 'edit' && !$scope.editing && !$scope.saving;
            }

            function canEdit() {
                return !$scope.readonly && $scope.mode !== 'view' && !$scope.editing && !$scope.saving;
            }

            function canEditLabels() {
                return !$scope.readonly && !$scope.editingLabels && !$scope.saving;
            }

            function canEditReferences() {
                return !$scope.readonly && $scope.mode !== 'view' && $scope.editing && !$scope.saving;
            }

            function canAddReference() {
                return canEditReferences &&
                    _.all($scope.purchase.referenceNumbers, function (ref) {
                        return ref.key && ref.value;
                    });
            }

            function canPrintLabels(form) {
                return form.$valid && _.any($scope.purchase.products, function (prod) {
                    return prod.labelRequired && prod.printQuantity > 0;
                });
            }

            function addProduct() {
                if (!canAddProducts()) {
                    return;
                }

                var newProduct = {
                    productId: null,
                    sku: '',
                    description: '',
                    quantityOrdered: 1
                };

                $scope.purchase.products.push(newProduct);
            }

            function save(form) {
                if (!canSave(form)) {
                    return;
                }

                $scope.saving = true;
                purchaseResourceProvider
                    .save($scope.purchase)
                    .then(
                    function (purchase) {
                        //if ($scope.purchase.Products.ResponseMsg != "DONE") {
                        debugger;
                            $scope.purchase = purchase;
                            updateMode(purchase);
                            updateTitle(purchase);
                            updateRoute(purchase);
                            pageHelper.notification.show('Purchase order saved successfully.');
                            $scope.saving = false;
                            $scope.editing = false;
                       // }
                        //else {
                          //  pageHelper.notification.show('Select Ashely Products');
                       // }
                    }, function (err) {
                        if (err.status !== 'success' && err.status !== 'error') {
                            pageHelper.notification.showPersistent(err.message);
                        }
                        $scope.saving = false;
                    });
            }

            function cancel() {
                if (!canCancel()) {
                    return;
                }

                $scope.purchase = beforeEditClone;
                $scope.editing = false;
                $scope.editingLabels = false;
            }

            function cancelOrder() {
                if (!canCancelOrder) {
                    return;
                }
                // Check click on CancelPO button cancel reason select or not 
                if ($scope.GetAshleyEnable == true) {
                    if ($scope.purchase.status != "New") {
                        if ($scope.purchase.cancelReasonId < 1 || $scope.purchase.cancelReasonId == null || $scope.purchase.cancelReasonId == undefined) {
                            pageHelper.notification.show("Please Select Cancellation Reason");
                            return;
                        }
                    }
                }
                $scope.saving = true;
                purchaseResourceProvider
                    .cancel($scope.purchase.id, ($scope.purchase.cancelReasonId == null || $scope.purchase.cancelReasonId == undefined) ? 0 : $scope.purchase.cancelReasonId)
                    .then(
                    function () {
                        $scope.purchase.status = 'Cancelled';
                        pageHelper.notification.show('Purchase Order Cancelled.');
                        updateMode($scope.purchase);
                        $scope.saving = false;
                        $scope.editing = false;
                    }, function (err) {
                        $scope.saving = false;
                    });
            }

            ///****************  Code Added by Abhijeet (Start) Approve & Reject PO 
            function ApproveOrder() {

                if ($scope.ApproveRejectOrderPermissionFirst && !$scope.ApproveRejectOrderPermissionSecond) {
                    purchaseResourceProvider
                        .firstapprove($scope.purchase.id)
                        .then(
                        function () {
                            $scope.purchase.status = 'Approved';
                            pageHelper.notification.show('Purchase order approved sucessfully');
                            updateMode($scope.purchase);
                            $scope.saving = false;
                            $scope.editing = false;
                        }, function (err) {
                            $scope.saving = false;
                        });
                }

                else if (!$scope.ApproveRejectOrderPermissionFirst && $scope.ApproveRejectOrderPermissionSecond) {
                    purchaseResourceProvider
                        .secondapprove($scope.purchase.id)
                        .then(
                        function () {
                            $scope.purchase.status = 'Approved';
                            pageHelper.notification.show('Purchase order approved sucessfully');
                            updateMode($scope.purchase);
                            $scope.saving = false;
                            $scope.editing = false;
                        }, function (err) {
                            $scope.saving = false;
                        });
                }
                else if ($scope.ApproveRejectOrderPermissionFirst && $scope.ApproveRejectOrderPermissionSecond) {
                    purchaseResourceProvider
                        .bothapprove($scope.purchase.id)
                        .then(
                        function () {
                            $scope.purchase.status = 'Approved';
                            pageHelper.notification.show('Purchase order approved sucessfully');
                            updateMode($scope.purchase);
                            $scope.saving = false;
                            $scope.editing = false;
                        }, function (err) {
                            $scope.saving = false;
                        });
                }
            }

            function RejectOrder() {
                if ($scope.ApproveRejectOrderPermissionFirst && !$scope.ApproveRejectOrderPermissionSecond) {
                    purchaseResourceProvider
                        .firstreject($scope.purchase.id)
                        .then(
                        function () {
                            $scope.purchase.status = 'Rejected';
                            pageHelper.notification.show('Purchase order rejected.');
                            updateMode($scope.purchase);
                            $scope.saving = false;
                            $scope.editing = false;
                        }, function (err) {
                            $scope.saving = false;
                        });
                }
                else if (!$scope.ApproveRejectOrderPermissionFirst && $scope.ApproveRejectOrderPermissionSecond) {
                    purchaseResourceProvider
                        .secondreject($scope.purchase.id)
                        .then(
                        function () {
                            $scope.purchase.status = 'Rejected';
                            pageHelper.notification.show('Purchase order rejected.');
                            updateMode($scope.purchase);
                            $scope.saving = false;
                            $scope.editing = false;
                        }, function (err) {
                            $scope.saving = false;
                        });
                }
                else if ($scope.ApproveRejectOrderPermissionFirst && $scope.ApproveRejectOrderPermissionSecond) {
                    purchaseResourceProvider
                        .bothreject($scope.purchase.id)
                        .then(
                        function () {
                            $scope.purchase.status = 'Rejected';
                            pageHelper.notification.show('Purchase order rejected.');
                            updateMode($scope.purchase);
                            $scope.saving = false;
                            $scope.editing = false;
                        }, function (err) {
                            $scope.saving = false;
                        });
                }
            }

            ///****************  Code Added by Abhijeet (End)

            function email() {

                $scope.saving = true;
                purchaseResourceProvider
                    .email($scope.purchase.id)
                    .then(
                    function () {
                        pageHelper.notification.show('Purchase order added to email queue.');
                        updateMode($scope.purchase);
                        $scope.saving = false;
                        $scope.editing = false;
                    }, function (err) {
                        $scope.saving = false;
                        pageHelper.notification.showPersistent(err.message);
                    });
            }

            function edit() {
                if (!canEdit()) {
                    return;
                }

                beforeEditClone = angular.copy($scope.purchase);
                $scope.editing = true;
            }

            function editLabels() {
                if (!canEditLabels()) {
                    return;
                }
                beforeEditClone = angular.copy($scope.purchase);
                $scope.editing = true;
                $scope.editingLabels = true;
            }

            function removeProduct(idx) {
                $scope.purchase.products.splice(idx, 1);
            }

            function updateTitle(purchase) {
                var title;
                if (purchase.id) {
                    title = 'Purchase Order #' + purchase.id;
                    if (purchase.originSystem && purchase.originSystem !== 'COSACS') {
                        title += ' (' + purchase.originSystem + ' #' + purchase.corporatePoNumber + ')';
                    }
                } else {
                    title = 'Create Purchase Order';
                }
                pageHelper.setTitle(title);
            }

            function updateRoute(purchase) {
                $location.path(url.resolve('Merchandising/Purchase/Detail/' + purchase.id));
            }

            function addReferenceNumber() {
                if (canAddReference()) {
                    $scope.purchase.referenceNumbers.push({
                        name: 'Container Number',
                        number: ''
                    });
                }
            }

            function removeReferenceNumber(referenceNumber) {
                $scope.purchase.referenceNumbers = _.reject($scope.purchase.referenceNumbers, function (n) { return n === referenceNumber; });
            }

            function lineCost(product) {
                return (product.quantityOrdered || 0) * (product.unitCost || 0);
            }

            function totalCost(products) {
                return total(products, function (product) {
                    return Number.parseFloat(lineCost(product));
                });
            }

            function total(list, iteratee) {
                return _.chain(list)
                    .map(iteratee)
                    .reduce(function (mem, num) {
                        return mem + num;
                    }, 0)
                    .value();
            }

            function print(includeCost) {
                confirm('This action will be audited. Would you like to continue?',
                    'Audit Confirmation', function (agree) {
                        if (agree) {
                            if (includeCost) {
                                url.open('/Merchandising/Purchase/PrintWithCost/' + $scope.purchase.id);
                            } else {
                                url.open('/Merchandising/Purchase/Print/' + $scope.purchase.id);
                            }
                        }
                    });
            }

            function printAlternate() {
                url.open('/Merchandising/Purchase/PrintAlternate/' + $scope.purchase.id);
            }

            function canPrint() {
                return !!window.bluePrintRawBase64;
            }

            function printLabels() {
                $scope.saving = true;
                purchaseResourceProvider.getLabels($scope.purchase.products).then(
                    function (result) {
                        var printingSuccessful = window.bluePrintRawBase64(result.printer, result.data);

                        if (printingSuccessful.success) {
                            pageHelper.notification.show('Printing stock labels successful.');
                        } else {
                            var message = "";
                            if (printingSuccessful.error && printingSuccessful.error.length > 0) {
                                message = " : " + printingSuccessful.error;
                            }

                            pageHelper.notification.showPersistent('Printing stock labels unsuccessful' + message);
                        }

                        $scope.saving = false;
                        $scope.editing = false;
                        $scope.editingLabels = false;
                    },
                    function () {
                        $scope.saving = false;
                        $scope.editing = false;
                        $scope.editingLabels = false;
                    });
            }

            function initializeEditMode() {
                if ($scope.mode === 'create') {
                    $scope.purchase.originSystem = 'COSACS';
                    edit();
                }
            }

            function initializeReferenceNumbers(purchase) {
                if (purchase.referenceNumbers && purchase.referenceNumbers.length < 1) {
                    addReferenceNumber();
                }
            }

            function initializePageData() {
                pageHelper.getSettings(settingsSources, function (options) {
                    $scope.options = options;
                    $scope.$apply();
                });


                vendorResourceProvider.getList().then(function (vendors) {
                    $scope.vendors = vendors;
                });

                vendorResourceProvider.get().then(function (vendors) {
                    $scope.vendorsFull = vendors;
                });

                locationResourceProvider.getList().then(function (locations) {
                    $scope.locations = locations;
                });


                // Code Added By Abhijeet for Cancel PO 
                purchaseResourceProvider.getList().then(function (POCancelReasonList) {
                    $scope.POCancelReasonList = POCancelReasonList;
                });

                // Code Added By Abhijeet for Ashley Enabled/ Disabled Flag
                purchaseResourceProvider.GetAshleyEnable().then(function (GetAshleyEnable) {
                    $scope.GetAshleyEnable = GetAshleyEnable;
                });

               
            }

            function generateUrl(link) {
                return url.resolve(link);
            }

            function updateLocation() {
                if ($scope.purchase) {
                    $scope.purchase.location = $scope.locations[$scope.purchase.locationId];
                }
            }

            function updateVendor() {                
                $scope.purchase.vendor = _.find($scope.vendorsFull, function (vendor) {
                    return vendor.id == $scope.purchase.vendorId;
                });
                $scope.purchase.paymentTerms = $scope.purchase.vendor.paymentTerms; 
                              
                // Code Added by Abhijeet for Ashley
                // Check Condition change Vendor accodingly change vendor price present in Additional Cost Price Table 
                // Code Added By Abhijeet for Ashley Enabled/ Disabled Flag

                if ($scope.purchase.status === "UnApproved") {
                    purchaseResourceProvider.GetACPPriceDetail($scope.purchase.vendorId).then(function (GetACPPriceDetail) {
                        $scope.GetACPPriceDetail = GetACPPriceDetail;
                        $scope.FindACPProductList = [];

                        if ($scope.GetACPPriceDetail == undefined || $scope.GetACPPriceDetail.length == 0) {
                            pageHelper.notification.show("No Addtional CP found for any product");
                        }
                        else {
                            
                            for (var i = 0; i < $scope.GetACPPriceDetail.length; i++) {
                                for (var j = 0; j < $scope.purchase.products.length; j++) {
                                    if ($scope.GetACPPriceDetail[i].productId == $scope.purchase.products[j].productId && $scope.GetACPPriceDetail[i].vendorId == $scope.purchase.vendorId) {
                                        $scope.FindACPProductList.push($scope.GetACPPriceDetail[i]);
                                    }
                                }
                            }
                        
                            if ($scope.FindACPProductList.length == $scope.purchase.products.length) {                               
                                for (var i = 0; i < $scope.GetACPPriceDetail.length; i++) {
                                    for (var j = 0; j < $scope.purchase.products.length; j++) {
                                        if ($scope.GetACPPriceDetail[i].productId == $scope.purchase.products[j].productId) {
                                            $scope.purchase.products[j].unitCost = $scope.GetACPPriceDetail[i].supplierCost;
                                            console.log($scope.GetACPPriceDetail[i].unitCost);
                                        }
                                    }
                                }
                            }
                            else {
                                pageHelper.notification.show("No Product Found For Selectecd Vendor");
                            }
                        }
                    });
                }
            }
            function cancelled() {
                return $scope.purchase.status === "Cancelled";
            }

            function closed() {
                return $scope.purchase.status === "Cancelled" || $scope.purchase.status === "Completed";
            }

            function isNew() {
                return $scope.purchase.status === "New";
            }

            function cancelProduct(lineItem) {
                var remain = lineItem.quantityOrdered - lineItem.totalQuantityReceived - lineItem.quantityCancelled;
                confirm('Are you sure you want to cancel the remaining ' + (remain > 1 ? remain + ' items?' : 'item?'),
                    'Cancel remaining products', function (agree) {
                        if (agree) {
                            purchaseResourceProvider.cancelProduct($scope.purchase.id, lineItem.id).then(function (updatedPurchase) {
                                $scope.purchase = updatedPurchase;
                            });
                        }
                    });
            }

            function canCancelAny() {
                return _.find($scope.purchase.products, function (product) {
                    return product.quantityOrdered - product.totalQuantityReceived - product.quantityCancelled > 0;
                }) !== null;
            }

            $scope.requestedDateChanged = function (date) {
                $scope.$broadcast('purchaseOrderDateChange', date);
            };

            $scope.isCreateMode = isCreateMode;
            $scope.isEditMode = isEditMode;
            $scope.isViewMode = isViewMode;
            $scope.save = save;
            $scope.canAddReference = canAddReference;
            $scope.canEditReferences = canEditReferences;
            $scope.addReferenceNumber = addReferenceNumber;
            $scope.removeReferenceNumber = removeReferenceNumber;
            $scope.canAddProducts = canAddProducts;
            $scope.canSave = canSave;
            $scope.canCancel = canCancel;
            $scope.canCancelOrder = canCancelOrder;
            $scope.canEdit = canEdit;
            $scope.addProduct = addProduct;
            $scope.cancel = cancel;
            $scope.email = email;
            $scope.cancelOrder = cancelOrder;
            $scope.edit = edit;
            $scope.removeProduct = removeProduct;
            $scope.lineCost = lineCost;
            $scope.totalCost = totalCost;
            $scope.print = print;
            $scope.generateUrl = generateUrl;
            $scope.updateVendor = updateVendor;
            $scope.updateLocation = updateLocation;
            $scope.cancelled = cancelled;
            $scope.closed = closed;
            $scope.isNew = isNew;
            $scope.canEditLabels = canEditLabels;
            $scope.editLabels = editLabels;
            $scope.printLabels = printLabels;
            $scope.canPrintLabels = canPrintLabels;
            $scope.canPrint = canPrint;
            $scope.cancelProduct = cancelProduct;
            $scope.canCancelAny = canCancelAny;
            $scope.printAlternate = printAlternate;
            initializePageData();
            // Code Added By Abhijeet 
            $scope.ApproveOrder = ApproveOrder;
            $scope.RejectOrder = RejectOrder;
        };
    });
