#!/bin/bash
    systemctl stop x-ui-multi-protocol
    systemctl disable x-ui-multi-protocol
    rm -f /etc/systemd/system/x-ui-multi-protocol.service 
    systemctl daemon-reload
    systemctl reset-failed
    rm -rf /etc/x-ui-multi-protocol 
    rm -rf x-ui-multi-protocol/x-ui-multi-protocol 
    sudo apt-get purge dotnet-sdk-8.0
     sudo rm -f /etc/apt/sources.list.d/microsoft-prod.list && sudo apt update

    echo "Uninstalled Successfully!"

