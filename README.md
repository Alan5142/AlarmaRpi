# Servidor
Este proyecto consiste en una alarma activada por infrarrojos, a grandes rasgos se compone de 2 partes
### Cliente
Puede ser cualquier MCU con soporte WiFi o Ethernet, se encarga de procesar si hay un objeto que interrumpio una señal
entre un emisor y un receptor de infrarrojos, una vez que esto pase envía un mensaje al servidor.
Esta pensado para proteger lugares en un establecimiento
### Servidor
Procesa los mensajes recibidos por el cliente, una vez que se activa la alarma, activa un buzzer piezoelectrico para emitir un sonido
y envía un correo a un destinatario avisando que se activo la alarma, para desactivarlo se necesita envíar un comando al servidor,
actualmente la única forma de hacerlo es mediante comandos por voz con Cortana de Windows 10
