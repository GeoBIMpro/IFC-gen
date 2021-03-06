
import {BaseIfc} from "./BaseIfc"
import {IfcReal} from "./IfcReal.g"
import {IfcPresentationItem} from "./IfcPresentationItem.g"

/**
 * http://www.buildingsmart-tech.org/ifc/IFC4/final/html/link/ifcsurfacestylerefraction.htm
 */
export class IfcSurfaceStyleRefraction extends IfcPresentationItem {
	RefractionIndex : IfcReal // optional
	DispersionFactor : IfcReal // optional

    constructor() {
        super()
    }
    getStepParameters() : string {
        var parameters = new Array<string>();
		parameters.push(BaseIfc.toStepValue(this.RefractionIndex))
		parameters.push(BaseIfc.toStepValue(this.DispersionFactor))

        return parameters.join();
    }
}