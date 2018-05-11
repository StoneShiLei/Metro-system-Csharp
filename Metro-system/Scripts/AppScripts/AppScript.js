$(document).ready(function () {
    $("#creatCard").on("click", creatCard);
    $("#destroyCard").on("click", destroyCard);


    function creatCard() {
        $.ajax({
            type: "POST",
            url: "/card/creatcard/",
            //data: {
            //    "org.id": "${org.id}"
            //},
            success: function (result) {
                if (result.ResultInfoEnum === "OK") {
                    alert("创建成功，id:" + result.Data.CardId)
                } else {
                    alert("创建失败:" + result.Message)
                }
            }
        });
    };

    function destroyCard() {
        $.ajax({
            type: "POST",
            url: "/card/destroycard/",

            success: function (result) {
                if (result.ResultInfoEnum === "OK") {
                    alert("删除成功!")
                } else {
                    alert("创建失败:" + result.Message)
                }
            }
        });
    };
}) 