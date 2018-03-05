var emailValidator = function (address) {
    var _validatedByPeriod = new RegExp("^\\.|\\.\\.|\\.$");
    var _localPartValidatedByAsciiCharacter = new RegExp("^\"(?:.)*\"$");
    var _localPartValidatedByIncorrectUse= new RegExp("(?:.)+[^\\\\]\"(?:.)+");
    var _localPartValidatedByCorrectUse = new RegExp("[ @\\[\\]\\\\\",]","g");
    var _domainPartValidatedByPeriod = new RegExp("^\\[(.)+]$");
    var _ipValidator = new RegExp("\\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$","g");
    var _domainPartValidatedByCorrectUse = new RegExp("(?:[0-9a-zA-Z][0-9a-zA-Z-]{0,61}[0-9a-zA-Z]|[a-zA-Z])(?:\\.|$)|(.)");
    var _validatedByHyphen = new RegExp("^\\-|\\-$");
    var _validatedByNumber = new RegExp("^[0-9]+$");

    if (address.indexOf("@") < 1)
    {
        alert();
        return false;
    }

    var partIndex = address.lastIndexOf("@");

    if (partIndex == address.length - 1)
    {
        alert();
        return false;
    }
    var localPart = address.substr(0, partIndex);
    var domainPart = address.substring(partIndex + 1);

    if (localPart.length < 1 || localPart.length > 64) {
        return false;
    }
    if (domainPart.length < 1 || domainPart.length > 255) {
        return false;
    }

    if (_validatedByPeriod.test(localPart))
    {
        return false;
    }

    if (_localPartValidatedByAsciiCharacter.test(localPart)) {
        if (_localPartValidatedByIncorrectUse.test(localPart))
        {
            return false;
        }
    }
    else {
        if (_localPartValidatedByCorrectUse.test(localPart))
        {
            var st = localPart.replace(_localPartValidatedByCorrectUse, "");
            if (_localPartValidatedByCorrectUse.test(st))
            {
                return false;
            }
        }
    }

    if (_domainPartValidatedByPeriod.test(domainPart)) {
        var ip = domainPart.substr(1, domainPart.length - 2);
        var matchesIp = _ipValidator.exec(ip);
        if (matchesIp.length > 0) {
            if (ip != matchesIp[0]) {
                if (ip.lastIndexOf(":") == ip.length - 1)
                {
                    return false;
                }
                if (ip.substring(0, 4) != "IPv6:") {
                    return false;
                }
            }
        }
        else {
            if (ip.substring(0, 4) != "IPv6:") {
                return false;
            }
        }
    }
    else {
        if (domainPart.indexOf(".") < 0)
        {
            return false;
        }
        if (_validatedByPeriod.test(domainPart))
        {
            return false;
        }
        if (!_domainPartValidatedByCorrectUse.test(domainPart))
        {
            return false;
        }
        var parts = domainPart.split('.');
        for (var p in parts)
        {
            if (_validatedByHyphen.test(p))
            {
                return false;
            }
        }
        if (_validatedByNumber.test(parts[parts.length - 1]))
        {
            return false;
        }

    }
    return true;
}
