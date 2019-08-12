Contenido
Guía para crear una aplicación multicontenedor usando ASP.NET Core y Docker y cómo desplegar en el clúster Kubernetes de Azure.	1
Solución Visual Studio compuesta de 2 proyectos	1
Contenidos en Github	1
Definición de imágenes Docker	3
Dockerfiles	3
Docker compose	3
Publicar imágenes docker a docker hub	3
Despliega la aplicación multi-contenedor en el clúster Azure Kubernetes.	3
Usa el objeto configmap en kubernetes para definir datos no sensibles	3
Usa el objeto secret en kubernetes para definir datos sensibles	4
Despliega la aplicación multi-contenedor a Kubernetes	4
Configuración de Nginx Ingress Controller para terminación SSL del servicio Frontend.	5
Servicios de Azure	6


















Guía para crear una aplicación multicontenedor usando ASP.NET Core y Docker y para desplegar en un clúster Kubernetes de Azure.

La guía te enseña a:
-	Usar variables de entorno en un dockerfile, un docker compose y plantillas de Kubernetes para especificar las configuraciones de la aplicación.
-	Configurar el despliegue para monitorearlo usando Azure App Insights.
-	Delegar un dominio público a Azure DNS.

Solución Visual Studio compuesta de 2 proyectos

-	TodoWeb: ASP.NET Core web app utilizado como Front End en la solución. Proyecto compuesto de páginas Razor que pueden ser usados para ver la colección de elementos To-do almacenados en una colección de Azure Cosmos DB. El Front envía logs, eventos, traces, peticiones, dependencias y excepciones a Application Insights.
-	TodoApi: ASP-NET Core web api invocado por servicio front-end servicio para acceder al dato almacenado en un Azure Cosmo DB SQL API database. Cada vez que una operación CRUD es realizada por alguno de los métodos expuestos por el TodoController, el servicio backend envía un mensaje de notificación a una cola del Aazure Service Bus. El servicio back-end envía logs, eventos, trazas, peticiones, dependencias y excepciones a Application Insight. El backend adopta swagger para exponer los APIs.

Contenidos en Github
El código fuente está ubicado en el repositorio Git https://github.com/lurrelo/AzureK8S-multi-container-app y su contenido está estructurado de la siguiente forma:
-	Scripts
o	Carpeta aks-kubernetes-cluster contiene el script Create-kubernetes-aks-cluster.cmd usado para crear un clúster AKS.
o	Carpeta Push-docker-image-scripts contiene el script Push-image-to-docker-hub.cmd usado para realizar la publicación de las imágenes docker hacia un repositorio en docker hub.
o	Carpeta Azure-DNS contiene el script create-azure-dns-for-kubernetes-todoapi-service.cmd usado para un servicio Azure DNS en la nube, responsable de resolver el nombre del sitio web a su dirección IP

-	Scripts/Kubernetes-scripts
o	Create-application-in-kubernetes-from-docker-hub.cmd: script crea los servicios y despliegues que componen la aplicación multi-contenedor. Este script extrae la información de las imágenes docker desde docker hub usando las definiciones configuradas en el archivo todolist-deployment-ans-services-from-docker-hub.yml.
o	Create-todolist-configmap.cmd: script crea el objeto todolist-configmap en el clúster Kubernetes usando el archivo descriptivo todolist-configmap.yml que almacena datos de configuración no sensible usados por la aplicación.
o	Create-todolist-secret.cmd: script crea el objeto todolist-secret en el clúster kubernetes usando el archivo descriptivo todolist-secret.yml que contiene datos de configuración sensible usado por la aplicación.
o	Install nginx-ingress-controller.sh: script usado para instalar el ingress controller NGINX en el clúster Kubernetes.
o	Scale-nginx-ingress-controller-replicas.sh: script usado para escalar el número de réplicas usados por el ingress controller NGINX.
o	Install-openssl.sh: script usado para instalar la utilidad OpenSSL.
o	Create-certificate.sh: script usado para crear un certificado de prueba para kubernetes.
o	Create-tls-secret.sh: script usado para crear un secreto en tu clúster kubernetes usando el certificado auto firmado.

-	Scripts/Helm
o	Install-helm.sh: Bash script usado para instalar e inicializar Helm, una herramienta para manejar charts de Kubernetes.

Nota: Servicios dockerizados usan la imagen base Microsoft/aspnetcore:2.0

Configuración:
/TodoApi/appsetting.json
-	Repository Service: La configuración de este elemento, sirve para la conexión de BD y el almacenamiento de datos. Para mayor detalle del servicio Azure Cosmos DB, revisar https://azure.microsoft.com/es-es/services/cosmos-db/
-	Notification Service: La configuración de este elemento, sirve para la mensajería y el servicio de colas donde el servicio backend envía un mensaje por cada operación CRUD realizada. Para mayor detalle del servicio Azure Cosmos DB, revisar https://azure.microsoft.com/es-es/services/service-bus/
-	Data Protection: La configuración de este elemento, sirve para el uso del sistema de protección de datos. Para mayor detalle del servicio Azure Cosmos DB, revisar https://azure.microsoft.com/es-es/services/storage/blobs/
-	Application Insight: La configuración de este elemento, sirve para el diagnóstico, monitoreo de desempeño, analítica y alertas.
-	Logging: Contiene la configuración de los niveles de logs para todos los proveedores Logging.
/TodoWeb/appsetting.json
Se trata de la misma configuración.

Definición de imágenes Docker
Dockerfiles
Existen 2 archivos dockerfiles, 1 para el servicio Front (/TodoWeb/Dockerfile) y otro para el servicio Back (/TodoApi/Dockerfile). Para mayor información, visitar https://docs.docker.com/engine/reference/builder/
Docker compose
El archivo docker-compse.yml es responsable de orquestar los servicios dockerizados de la aplicación. Se utiliza para realizar pruebas locales de la aplicación. Para mayor información, visitar https://docs.docker.com/compose/
El archivo docker-compose-override.yml incluye las variables de entorno con la finalidad de hacer pruebas locales sin depender de la configuración de los archivos appsetting.json ni de las actualizaciones en el código de la aplicación.
Recuerda que: antes de ejecutar el docker-compose-override.yml debes actualizar los valores solicitados asociados a los servicios de Azure ya explicados en secciones anteriores.

Publicar imágenes docker a docker hub
Utiliza el script Push-image-to-docker-hub.cmd para etiquetar y registrar las imágenes en tu repositorio en docker hub. Recuerda reemplazar las siguientes variables en el script:
-	Cambia DOCKER_HUB_REPOSITORY por nombre de usuario de docker hub
-	Cambiar DOCKER_HUB_PASSWORD por la clave de acceso a tu cuenta en docker hub.

Despliega la aplicación multi-contenedor en el clúster Azure Kubernetes.
Antes de todo, debemos crear el servicio Azure Kubernetes. Para ello, usaremos la interfaz de línea de comando de Azure (Az CLI) y luego Kubectl para el manejo de objetos dentro del clúster kubernetes. Para mayor información, visita 
https://docs.microsoft.com/es-es/cli/azure/?view=azure-cli-latest https://kubernetes.io/docs/reference/kubectl/overview/
Para la creación de kubernetes en Azure, usa el script create-kubernetes-aks-cluster.cmd.
Nota: Si quieres evitar la instalación de Azure CLI y Kubectl, puedes usar Azure Cloud Shell, se trata de un contenedor web público que ya tiene instaladas las herramientas. 
Para mayor información, visita https://docs.microsoft.com/en-us/azure/cloud-shell/overview
Usa el objeto configmap en kubernetes para definir datos no sensibles
Configmaps son entidades usadas en kubernetes para desacoplar los datos de configuración no sensibles desde la imagen y plantillas usadas para desplegar la aplicación. 
Para definir un configmap, debes crear un archivo yaml. El archivo todolist-configmap.yml define valores para los servicios de azure previamente comentados, además del entorno.
Para mayor información, visita https://kubernetes.io/docs/tasks/configure-pod-container/configure-pod-configmap/
Una vez definido, debes crearlos usando el script create-todolist-configmap.cmd.
Usa el objeto secret en kubernetes para definir datos sensibles
Secret es un objeto que contiene una pequeña porción de datos sensibles (passwords, tokens, certificados, etc.). Poner los valores sensibles en un objeto Secret te da un mejor control de su uso y reduce el riesgo de exponerlo accidentalmente. 
Para mayor información, visita https://kubernetes.io/docs/concepts/configuration/secret/
El primes paso es crear un archivo yaml para definir el secreto. Cada elemento en el archivo debe estar codificado en BASE64. Usa secretos para aquellos valores sensibles en los servicios de azure utilizados.
Usa el archivo todolist-secret.yaml para definir los secretos de los servicios de azure que se utilizan en la aplicación.
Recuerda, cambia los valores en el archivo todolist-secret.yaml por valores codificados en BASE64.
Para mayor información para convertir cadenas en BASE64, visita https://www.base64decode.org/
Para crear el objeto secreto, utiliza el script create-todolist-secret.cmd.

Despliega la aplicación multi-contenedor a Kubernetes
La propuesta del POC es desplegar la aplicación extrayendo las imágenes docker desde un repositorio del docker hub. Para ello, utilizar el archivo todolist-deployments-and-services-from-docker-hub.yml.
Configuración
Para usar un secreto en una variable de entorno en una especificación de un Pod, necesitas:
-	Crear un secreto. Multiples Pods pueden referenciar al mismo secreto.
-	Modificar tu definición de Pod en cada contenedor donde desees consumir el valor de un secreto para agregar una variable de entorno por cada secreto. Configura tu variable de entorno con el nombre y key de tu secreto.
Para más información, visita https://kubernetes.io/docs/concepts/configuration/secret/#using-secrets-as-environment-variables
Para desplegar la aplicación, utiliza el script create-application-in-kubernetes-from-docker-hub.cmd. Para validar la creación de tus objetos servicio y despliegue en el clúster de kubernetes, ejecuta los siguientes comandos:
-	Kubectl get services
-	Kubectl get deployments

Nota: usando el comando kubectl get services podrás conocer la IP pública asignadas a los servicios.
Para validar que la aplicación funciona, abre un browser y pegar la IP pública del servicio Front como se muestra en la imagen.
 

La guía no cubre configuraciones Frontend pero adjunto unos links de referencia sobre como asignar un servicio de Azure DNS a la IP pública del servicio Frontend:
https://docs.microsoft.com/en-gb/azure/dns/dns-operations-dnszones-cli
https://docs.microsoft.com/en-gb/azure/dns/dns-operations-recordsets-cli
https://docs.microsoft.com/en-us/azure/dns/dns-domain-delegation
https://docs.microsoft.com/en-us/azure/dns/dns-delegate-domain-azure-dns
Una vez creado el dominio, puedes utilizar el script create-azure-dns-for-kubernetes-todoapi-service.cmd para crear el DNS y registros relacionados.

Configuración de Nginx Ingress Controller para terminación SSL del servicio Frontend.
Otras configuraciones extras y que escapan al alcance de esta guía es la configuración del servicio Nginx Ingress Controller para la terminación SSL del servicio Frontend, ya que hasta el momento ambos servicios, tanto Frontend como Backend han sido desplegados en Kubernetes con una configuración de tipo LoadBalancer que expone públicamente la IP. En un escenario real, es importante configurar solo el acceso al servicio Frontend y usando un endpoint HTTPS. Por ello, se recomienda la configuración de terminación SSL, para ello se adjunta las siguientes lecturas:
https://kubernetes.io/docs/concepts/services-networking/ingress/
https://github.com/kubernetes/ingress-nginx
https://docs.helm.sh/
https://github.com/kubernetes/helm
https://github.com/kubernetes/ingress-nginx/blob/master/deploy/README.md
https://docs.microsoft.com/en-us/azure/aks/kubernetes-helm
https://github.com/kubernetes/ingress-nginx
Una vez terminadas las lecturas, puedes configurar la terminación usando los siguientes pasos:
1.	Instalar e inicializar Helm en el clúster de Kubernetes
o	Para esto, puedes usar el script install-helm.sh
2.	Instalar Nginx Ingress Controller
o	Para esto, puedes usar el script install-nginx-ingress-controller.sh
3.	Escalar el número de réplicas usados por el Ingress Controller
o	Para esto, puedes usar el script scale-nginx-ingress-controller-replicas.sh 
4.	Crear un secreto
o	Para esto, debes crear un certificado de prueba.
	Instala la utilidad open ssl usando el script install-open-ssl.sh.
	Crea un certificado autofirmado usando el script create-certificate.sh
	Crea un secreto usando el certificado autofirmado usando el script create-tls-secret.sh
Servicios de Azure
Azure Container Service (AKS) maneja el entorno de Kubernetes donde es rápido y fácil desplegar y manejar aplicaciones contenerizadas sin experiencia orquestando contenedores. Para mayor información, visita https://docs.microsoft.com/en-us/azure/aks/
Application Insights Es un servicio APM () para desarrolladores web en múltiples plataformas. Usalo para monitorear tu aplicación web en vivo, también detecta de manera automática anomalías de desempeño. Para mayor información, visita https://docs.microsoft.com/en-us/azure/application-insights/app-insights-overview
Service Bus Messaging Es un servicio de entrega de mensajes confiable. Cuándo 2 o mas partes quieren intercambiar datos, ellos necesitan un facilitador de comunicación. Para mayor información, visita https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview
Azure Cosmos DB es una BD multimodelo y distribuido globalmente por Microsoft. Cosmos DB te habilita escalar el rendimiento y el almacén de una manera independiente y elástica a cualquier región geográfica de Azure.