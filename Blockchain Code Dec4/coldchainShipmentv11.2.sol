pragma solidity ^0.4.20;


contract coldchainShipment {
    
	struct Shipment {
		bytes32     	shipmentId;
		string      	productList;     
		uint        	blockTimestamp;
		string          fromUser;
		
		ShipmentDetails shipmentDetails;
		MessageType 	msgType;
		ShipmentAlerts	shipmentAlerts;
	}

	struct ShipmentDetails {
		string 			sourceLocation;
		string			destinationLocation;
		string			logisticsPartner;
		bytes32			dateOfShipment;
		bytes32			dateOfDelivery;
		string			purchaseOrderNumber;
	}

	struct MessageType {
		string			messageType;
		string 			shipmentInfo;    
		string 			status;
		string 			docRef;          
		string 			heartBeat;       
	}
	
	struct ShipmentAlerts {
		int				alertCount; 
		string			alertDetails;
	}
	
	
	mapping(bytes32 => Shipment)ShipmentList;
	bytes32[] shipmentAccts;
	uint16 shipmentCount  = 0;
	uint16 totalAlerts = 0;
		
	
	/**************************** Event Declarations ****************************/
	
	event ShipmentEvent(
		bytes32 indexed shipmentId,
		uint   			blockTimestamp,
		string 			productList,
		
		string			messageType,
		string          fromUser
	);
	
	event ShipmentDetailsEvent(
		bytes32 indexed shipmentId,
		uint   			blockTimestamp,
		
		string 			sourceLocation,
		string			destinationLocation,
		string			logisticsPartner,
		bytes32			dateOfShipment,
		bytes32			dateOfDelivery,
		string 			purchaseOrderNumber,
		
		string			messageType,
		string          fromUser
	);
	
	event MsgTypeDetailsEvent (
	    bytes32 indexed shipmentId,
		uint   			blockTimestamp,
		
		string 			shipmentInfo,
		string 			status,
		string 			docRef,
		string 			heartBeat,
		
		string			messageType,
		string          fromUser
	);
	
	event ShipmentAlertsEvent (
		bytes32 indexed shipmentId,
		uint   			blockTimestamp,
		
		int 			alertCount,
		string			alertDetails,
		
		string			messageType,
		string          fromUser
	);

    
	/**************************** Setter Methods ****************************/
	
	function createShipment(bytes32 _shipmentId, string _productList,
							string _sourceLocation, string _destinationLocation, string _logisticsPartner, 
							bytes32 _dateOfShipment, bytes32 _dateOfDelivery, string _purchaseOrderNumber,
							string _messageType, string _fromUser
							) public
    {
        shipmentCount++;

        ShipmentList[_shipmentId].shipmentId            				= _shipmentId;
        ShipmentList[_shipmentId].productList           				= _productList;
        ShipmentList[_shipmentId].blockTimestamp        				=  block.timestamp;
        
        ShipmentList[_shipmentId].shipmentDetails.sourceLocation		= _sourceLocation;
        ShipmentList[_shipmentId].shipmentDetails.destinationLocation   = _destinationLocation;
		ShipmentList[_shipmentId].shipmentDetails.logisticsPartner      = _logisticsPartner;
		ShipmentList[_shipmentId].shipmentDetails.dateOfShipment        = _dateOfShipment;						 
		ShipmentList[_shipmentId].shipmentDetails.dateOfDelivery        = _dateOfDelivery;						 
		ShipmentList[_shipmentId].shipmentDetails.purchaseOrderNumber   = _purchaseOrderNumber;
		ShipmentList[_shipmentId].msgType.messageType					= _messageType;
		ShipmentList[_shipmentId].fromUser                              = _fromUser;
		
        shipmentAccts.push((bytes32) (_shipmentId));
        emitShipmentEvent(_shipmentId);
		emitShipmentDetailsEvent(_shipmentId);
    }
	
	function fillMsgTypeDetails(bytes32 _shipmentId, string _shipmentInfo, string _status, string _docRef, string _heartBeat, string _messageType, string _fromUser) public {
								 
		ShipmentList[_shipmentId].shipmentId            				= _shipmentId;
		ShipmentList[_shipmentId].blockTimestamp        				=  block.timestamp;
		ShipmentList[_shipmentId].msgType.shipmentInfo  				= _shipmentInfo;
        ShipmentList[_shipmentId].msgType.status        				= _status;
        ShipmentList[_shipmentId].msgType.docRef        				= _docRef;
        ShipmentList[_shipmentId].msgType.heartBeat     				= _heartBeat;
		ShipmentList[_shipmentId].msgType.messageType					= _messageType;
		ShipmentList[_shipmentId].fromUser                              = _fromUser;	
		
		emitMsgTypeDetailsEvent(_shipmentId);
	
	}							 
        
	
	/**************************** Getter Methods ****************************/
	
	function getShipments() public constant returns (bytes32[]){
        return shipmentAccts;
    }
    
    function getShipmentCount() public constant returns (uint16) {
        return shipmentCount;
    }
	
    function getShipmentById (bytes32 _shipmentId) public view returns (bytes32, string) {
        
        return (
			ShipmentList[_shipmentId].shipmentId,
			ShipmentList[_shipmentId].productList
        );
    }
	
    function getShipmentDetails(bytes32 _shipmentId) public constant returns (string, string, string, bytes32, bytes32, string){
        return (
			ShipmentList[_shipmentId].shipmentDetails.sourceLocation,		
			ShipmentList[_shipmentId].shipmentDetails.destinationLocation,
			ShipmentList[_shipmentId].shipmentDetails.logisticsPartner,    
			ShipmentList[_shipmentId].shipmentDetails.dateOfShipment,      
			ShipmentList[_shipmentId].shipmentDetails.dateOfDelivery,      
			ShipmentList[_shipmentId].shipmentDetails.purchaseOrderNumber 
		);
    }
	
	function getMsgTypeDetails(bytes32 _shipmentId) public constant returns (string, string, string, string){
        return (
			ShipmentList[_shipmentId].msgType.shipmentInfo,
			ShipmentList[_shipmentId].msgType.status,
			ShipmentList[_shipmentId].msgType.docRef,
			ShipmentList[_shipmentId].msgType.heartBeat
		);
    }
	
	function getShipmentAlerts(bytes32 _shipmentId) public constant returns (int, string) {
		return (
			ShipmentList[_shipmentId].shipmentAlerts.alertCount,
			ShipmentList[_shipmentId].shipmentAlerts.alertDetails
		);
	}
	
	
	/**************************** Update Methods ****************************/
	
	function updateShipmentInfo(bytes32 _shipmentId, string _shipmentInfo, string _messageType, string _fromUser) public {
        ShipmentList[_shipmentId].msgType.shipmentInfo 			= _shipmentInfo;
		ShipmentList[_shipmentId].blockTimestamp       			=  block.timestamp;
		ShipmentList[_shipmentId].msgType.messageType			= _messageType;
		ShipmentList[_shipmentId].fromUser                      = _fromUser;
        emitMsgTypeDetailsEvent(_shipmentId);
    }
    
    function updateShipmentStatus(bytes32 _shipmentId, string _status, string _messageType, string _fromUser) public {
        ShipmentList[_shipmentId].msgType.status 				= _status;
		ShipmentList[_shipmentId].blockTimestamp 				=  block.timestamp;
		ShipmentList[_shipmentId].msgType.messageType			= _messageType;
		ShipmentList[_shipmentId].fromUser                      = _fromUser;
        emitMsgTypeDetailsEvent(_shipmentId);
    }

    function updateShipmentDocs(bytes32 _shipmentId, string _docRef, string _messageType, string _fromUser) public {
        ShipmentList[_shipmentId].msgType.docRef 				= _docRef;
		ShipmentList[_shipmentId].blockTimestamp 				=  block.timestamp;
		ShipmentList[_shipmentId].msgType.messageType			= _messageType;
		ShipmentList[_shipmentId].fromUser                      = _fromUser;
        emitMsgTypeDetailsEvent(_shipmentId);
    }

    function updateShipmentHeartBeat(bytes32 _shipmentId, string _heartBeat, string _messageType, string _fromUser) public {
        ShipmentList[_shipmentId].msgType.heartBeat 			= _heartBeat;
		ShipmentList[_shipmentId].blockTimestamp    			=  block.timestamp;
		ShipmentList[_shipmentId].msgType.messageType			= _messageType;
		ShipmentList[_shipmentId].fromUser                      = _fromUser;
        emitMsgTypeDetailsEvent(_shipmentId);
    }
    
	function updateShipmentAlerts(bytes32 _shipmentId, uint16 _alertCount, string _alertDetails, string _messageType, string _fromUser) public {
        totalAlerts += _alertCount;
		ShipmentList[_shipmentId].shipmentAlerts.alertCount   	= totalAlerts;
		ShipmentList[_shipmentId].shipmentAlerts.alertDetails 	= _alertDetails;
		ShipmentList[_shipmentId].blockTimestamp       		  	=  block.timestamp;
		ShipmentList[_shipmentId].msgType.messageType			= _messageType;
		ShipmentList[_shipmentId].fromUser                      = _fromUser;
        emitShipmentAlertsEvent(_shipmentId);
    }
    
	
	/**************************** Event Methods ****************************/
	
	
	function emitShipmentEvent(bytes32 _shipmentId) public
    {
		emit ShipmentEvent (
			_shipmentId,
			ShipmentList[_shipmentId].blockTimestamp,
			ShipmentList[_shipmentId].productList,
			ShipmentList[_shipmentId].msgType.messageType,
			ShipmentList[_shipmentId].fromUser
		);
    }
	
	function emitShipmentDetailsEvent(bytes32 _shipmentId) public
    {
		emit ShipmentDetailsEvent (
			_shipmentId,
			ShipmentList[_shipmentId].blockTimestamp,
			
			ShipmentList[_shipmentId].shipmentDetails.sourceLocation,		
			ShipmentList[_shipmentId].shipmentDetails.destinationLocation,
			ShipmentList[_shipmentId].shipmentDetails.logisticsPartner,    
			ShipmentList[_shipmentId].shipmentDetails.dateOfShipment,      
			ShipmentList[_shipmentId].shipmentDetails.dateOfDelivery,      
			ShipmentList[_shipmentId].shipmentDetails.purchaseOrderNumber,
			ShipmentList[_shipmentId].msgType.messageType,
			ShipmentList[_shipmentId].fromUser
		);
    }
	
	function emitMsgTypeDetailsEvent(bytes32 _shipmentId) public 
	{
		emit MsgTypeDetailsEvent (
		    _shipmentId,
			ShipmentList[_shipmentId].blockTimestamp,
			ShipmentList[_shipmentId].msgType.shipmentInfo,
			ShipmentList[_shipmentId].msgType.status,
			ShipmentList[_shipmentId].msgType.docRef,
			ShipmentList[_shipmentId].msgType.heartBeat,
			ShipmentList[_shipmentId].msgType.messageType,
			ShipmentList[_shipmentId].fromUser
		);
	}
	
	function emitShipmentAlertsEvent(bytes32 _shipmentId) public
	{
		emit ShipmentAlertsEvent(
			_shipmentId,
			ShipmentList[_shipmentId].blockTimestamp,
			ShipmentList[_shipmentId].shipmentAlerts.alertCount,
			ShipmentList[_shipmentId].shipmentAlerts.alertDetails,
			ShipmentList[_shipmentId].msgType.messageType,
			ShipmentList[_shipmentId].fromUser
		);
	}
    
}